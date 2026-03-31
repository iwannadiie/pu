using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private Button _lastSelectedButton;
        private readonly int _currentTutorId = 1;
        private TutorDbContext _context;
        private Репетитор _currentTutor;
        private DateTime _currentDate = DateTime.Now;
        private Dictionary<DateTime, List<Занятие>> _lessonsByDate;
        private List<Ученик> _students;
        private List<УчебныйМатериал> _materials;
        private List<ИсторияСделки> _history;
        private DateTime _selectedDate = DateTime.Now;

        // Таймер для автоматического обновления
        private DispatcherTimer _updateTimer;

        // Элементы календаря
        private Button _prevMonthButton;
        private Button _nextMonthButton;
        private TextBlock _monthYearText;
        private UniformGrid _daysGrid;
        private Dictionary<Button, DateTime> _dayButtons = new Dictionary<Button, DateTime>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = new TutorDbContext("Server=DESKTOP;Database=Online_School;Integrated Security=True;");

                // Проверяем прошедшие занятия при загрузке
                await _context.UpdatePastLessonsAsync();

                await LoadTutorProfile();
                await LoadMonthLessons();
                await LoadStudents();
                await LoadMaterials();
                await LoadHistory();

                _lastSelectedButton = BtnCalendar;
                ShowCalendarContent();

                // Запускаем таймер для проверки каждую минуту
                StartUpdateTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Останавливаем таймер при закрытии окна
            _updateTimer?.Stop();
        }

        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMinutes(1); // Проверка каждую минуту
            _updateTimer.Tick += async (s, e) => await UpdateTimer_Tick();
            _updateTimer.Start();
        }

        private async Task UpdateTimer_Tick()
        {
            try
            {
                // Проверяем прошедшие занятия
                await _context.UpdatePastLessonsAsync();

                // Обновляем данные
                await LoadMonthLessons();
                await LoadHistory();

                // Обновляем отображение в зависимости от текущей вкладки
                if (_lastSelectedButton?.Tag?.ToString() == "Calendar")
                {
                    // Обновляем календарь
                    FillDaysGrid();

                    // Обновляем занятия на текущую дату
                    var currentDate = DateTime.Now.Date;
                    if (ContentPanel.Children.Count > 2)
                    {
                        ContentPanel.Children.RemoveAt(2);
                    }
                    await ShowLessonsForDate(currentDate);
                }
                else if (_lastSelectedButton?.Tag?.ToString() == "History")
                {
                    // Обновляем историю
                    ShowHistoryContent();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в таймере: {ex.Message}");
            }
        }

        private async Task LoadTutorProfile()
        {
            _currentTutor = await _context.GetTutorByIdAsync(_currentTutorId);

            if (_currentTutor != null)
            {
                AvatarText.Text = _currentTutor.Инициалы;
                TutorNameText.Text = _currentTutor.ПолноеИмя;
                TutorSpecText.Text = _currentTutor.Предметы;
                TutorRatingText.Text = $"Рейтинг: {_currentTutor.Рейтинг ?? 0} ★";
            }
        }

        private async Task LoadMonthLessons()
        {
            var startDate = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var всеЗанятия = await _context.GetLessonsForMonthAsync(_currentTutorId, startDate, endDate);

            _lessonsByDate = new Dictionary<DateTime, List<Занятие>>();
            foreach (var занятие in всеЗанятия)
            {
                if (!_lessonsByDate.ContainsKey(занятие.ДатаЗанятия))
                    _lessonsByDate[занятие.ДатаЗанятия] = new List<Занятие>();

                _lessonsByDate[занятие.ДатаЗанятия].Add(занятие);
            }
        }

        private async Task LoadStudents()
        {
            _students = await _context.GetAllStudentsAsync();
        }

        private async Task LoadMaterials()
        {
            _materials = await _context.GetStudyMaterialsAsync();
        }

        private async Task LoadHistory()
        {
            _history = await _context.GetLessonHistoryAsync(_currentTutorId);
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = sender as Button;
            if (clickedButton == null || clickedButton == _lastSelectedButton) return;

            if (_lastSelectedButton != null)
            {
                _lastSelectedButton.ClearValue(Button.BackgroundProperty);
                _lastSelectedButton.ClearValue(Button.ForegroundProperty);
            }

            clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            clickedButton.Foreground = Brushes.White;
            _lastSelectedButton = clickedButton;

            switch (clickedButton.Tag?.ToString())
            {
                case "Calendar": ShowCalendarContent(); break;
                case "Students": ShowStudentsContent(); break;
                case "Materials": ShowMaterialsContent(); break;
                case "History": ShowHistoryContent(); break;
                case "Profile": ShowProfileContent(); break;
            }
        }

        private async void ShowCalendarContent()
        {
            ContentPanel.Children.Clear();
            _dayButtons.Clear();

            ContentPanel.Children.Add(new TextBlock
            {
                Text = "Календарь занятий",
                Style = (Style)FindResource("SectionHeaderStyle")
            });

            // Навигация
            var navBorder = new Border { Style = (Style)FindResource("CardStyle"), Margin = new Thickness(0, 0, 0, 20) };
            var navStack = new StackPanel();
            var navGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };

            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _prevMonthButton = new Button
            {
                Content = "Предыдущий",
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = "prev"
            };
            _prevMonthButton.Click += MonthNavigation_Click;
            Grid.SetColumn(_prevMonthButton, 0);
            navGrid.Children.Add(_prevMonthButton);

            _monthYearText = new TextBlock
            {
                Text = _currentDate.ToString("MMMM yyyy"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_monthYearText, 1);
            navGrid.Children.Add(_monthYearText);

            _nextMonthButton = new Button
            {
                Content = "Следующий",
                Padding = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = "next"
            };
            _nextMonthButton.Click += MonthNavigation_Click;
            Grid.SetColumn(_nextMonthButton, 2);
            navGrid.Children.Add(_nextMonthButton);

            navStack.Children.Add(navGrid);

            // Дни недели
            var daysGrid = new Grid { Margin = new Thickness(0, 10, 0, 10) };
            for (int i = 0; i < 7; i++)
                daysGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
            for (int i = 0; i < 7; i++)
            {
                var dayText = new TextBlock
                {
                    Text = days[i],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"))
                };
                Grid.SetColumn(dayText, i);
                daysGrid.Children.Add(dayText);
            }
            navStack.Children.Add(daysGrid);

            _daysGrid = new UniformGrid { Rows = 6, Columns = 7, Margin = new Thickness(0, 5, 0, 0) };
            FillDaysGrid();
            navStack.Children.Add(_daysGrid);

            navBorder.Child = navStack;
            ContentPanel.Children.Add(navBorder);

            // Устанавливаем выбранную дату
            _selectedDate = DateTime.Now.Date;
            await ShowLessonsForDate(_selectedDate);
        }

        private void FillDaysGrid()
        {
            _daysGrid.Children.Clear();

            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            firstDayOfWeek = firstDayOfWeek == 0 ? 6 : firstDayOfWeek - 1;

            for (int i = 0; i < firstDayOfWeek; i++)
                _daysGrid.Children.Add(new Button { Visibility = Visibility.Hidden });

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
                var dayButton = new Button
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(2),
                    Background = Brushes.Transparent,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = date
                };

                // Подсветка выбранного дня (синим)
                if (date.Date == _selectedDate.Date)
                {
                    dayButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                    dayButton.Foreground = Brushes.White;
                    dayButton.FontWeight = FontWeights.Bold;
                    dayButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2980B9"));
                }
                // Подсветка текущего дня (светло-серым, если не выбран)
                else if (date.Date == DateTime.Now.Date)
                {
                    dayButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                    dayButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7"));
                    dayButton.FontWeight = FontWeights.SemiBold;
                }

                if (_lessonsByDate != null && _lessonsByDate.ContainsKey(date))
                {
                    var count = _lessonsByDate[date].Count;
                    dayButton.ToolTip = $"Занятий: {count}";

                    var stackPanel = new StackPanel();
                    var dayText = new TextBlock
                    {
                        Text = day.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = date.Date == _selectedDate.Date ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"))
                    };
                    stackPanel.Children.Add(dayText);

                    var indicator = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(count >= 3 ? "#E74C3C" : (count >= 2 ? "#F39C12" : "#2ECC71"))),
                        CornerRadius = new CornerRadius(4),
                        Height = 4,
                        Width = 20,
                        Margin = new Thickness(0, 2, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    stackPanel.Children.Add(indicator);
                    dayButton.Content = stackPanel;
                }
                else
                {
                    // Создаем TextBlock для числа, чтобы можно было менять цвет
                    var dayText = new TextBlock
                    {
                        Text = day.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    if (date.Date == _selectedDate.Date)
                    {
                        dayText.Foreground = Brushes.White;
                    }
                    else
                    {
                        dayText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                    }

                    dayButton.Content = dayText;
                }

                dayButton.Click += DayButton_Click;
                _dayButtons[dayButton] = date;
                _daysGrid.Children.Add(dayButton);
            }
        }

        private async void MonthNavigation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            button.IsEnabled = false;

            try
            {
                _currentDate = button.Tag.ToString() == "prev" ?
                    _currentDate.AddMonths(-1) : _currentDate.AddMonths(1);

                _monthYearText.Text = _currentDate.ToString("MMMM yyyy");

                await LoadMonthLessons();
                FillDaysGrid();

                // Показываем занятия на выбранную дату (если она есть в новом месяце)
                if (_selectedDate.Month == _currentDate.Month && _selectedDate.Year == _currentDate.Year)
                {
                    await ShowLessonsForDate(_selectedDate);
                }
                else
                {
                    // Если выбранная дата не в текущем месяце, показываем первый день
                    var firstDay = new DateTime(_currentDate.Year, _currentDate.Month, 1);
                    _selectedDate = firstDay;
                    await ShowLessonsForDate(firstDay);
                }
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void DayButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is DateTime date)
            {
                _selectedDate = date;

                // Обновляем подсветку всех кнопок
                foreach (var btn in _dayButtons.Keys)
                {
                    var btnDate = _dayButtons[btn];

                    if (btnDate.Date == _selectedDate.Date)
                    {
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
                        btn.Foreground = Brushes.White;
                        btn.FontWeight = FontWeights.Bold;
                        btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2980B9"));

                        // Обновляем цвет текста в зависимости от типа содержимого
                        if (btn.Content is StackPanel stack)
                        {
                            if (stack.Children[0] is TextBlock text)
                                text.Foreground = Brushes.White;
                        }
                        else if (btn.Content is TextBlock textBlock)
                        {
                            textBlock.Foreground = Brushes.White;
                        }
                    }
                    else if (btnDate.Date == DateTime.Now.Date)
                    {
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                        btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                        btn.FontWeight = FontWeights.SemiBold;
                        btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7"));

                        if (btn.Content is StackPanel stack)
                        {
                            if (stack.Children[0] is TextBlock text)
                                text.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                        }
                        else if (btn.Content is TextBlock textBlock)
                        {
                            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                        }
                    }
                    else
                    {
                        btn.ClearValue(Button.BackgroundProperty);
                        btn.ClearValue(Button.ForegroundProperty);
                        btn.FontWeight = FontWeights.Normal;
                        btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7"));

                        if (btn.Content is StackPanel stack)
                        {
                            if (stack.Children[0] is TextBlock text)
                                text.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                        }
                        else if (btn.Content is TextBlock textBlock)
                        {
                            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
                        }
                    }
                }

                await ShowLessonsForDate(date);
            }
        }

        private async Task ShowLessonsForDate(DateTime date)
        {
            // Удаляем предыдущий контент (последний элемент - карточка с занятиями)
            while (ContentPanel.Children.Count > 2)
            {
                ContentPanel.Children.RemoveAt(2);
            }

            var занятия = await _context.GetLessonsForDateAsync(_currentTutorId, date);

            var lessonsBorder = new Border { Style = (Style)FindResource("CardStyle") };
            var lessonsStack = new StackPanel();

            lessonsStack.Children.Add(new TextBlock
            {
                Text = $"Занятия на {date:dd MMMM yyyy}:",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                Margin = new Thickness(0, 0, 0, 15)
            });

            if (занятия != null && занятия.Count > 0)
            {
                foreach (var занятие in занятия)
                {
                    var card = CreateLessonCard(занятие);
                    lessonsStack.Children.Add(card);
                }
            }
            else
            {
                lessonsStack.Children.Add(new TextBlock
                {
                    Text = "На этот день занятий нет",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(20)
                });
            }

            lessonsBorder.Child = lessonsStack;
            ContentPanel.Children.Add(lessonsBorder);
        }

        private Border CreateLessonCard(Занятие занятие)
        {
            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 8),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // Время - темно-синий
            var timeText = new TextBlock
            {
                Text = занятие.Время,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2980B9")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(timeText, 0);
            grid.Children.Add(timeText);

            // Ученик - черный
            var studentText = new TextBlock
            {
                Text = занятие.Ученик?.ПолноеИмя ?? "Неизвестно",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(studentText, 1);
            grid.Children.Add(studentText);

            // Тема - темно-серый
            var topicText = new TextBlock
            {
                Text = занятие.Тема ?? "Без темы",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(topicText, 2);
            grid.Children.Add(topicText);

            // Статус - цветной фон с белым текстом
            var statusBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(занятие.СтатусЦвет)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 3, 8, 3),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var statusText = new TextBlock
            {
                Text = занятие.Статус,
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };

            statusBorder.Child = statusText;
            Grid.SetColumn(statusBorder, 3);
            grid.Children.Add(statusBorder);

            border.Child = grid;
            return border;
        }

        private void ShowStudentsContent()
        {
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = "Мои ученики",
                Style = (Style)FindResource("SectionHeaderStyle")
            });

            if (_students != null && _students.Count > 0)
            {
                foreach (var student in _students)
                {
                    var card = new Border { Style = (Style)FindResource("StudentCardStyle") };
                    var grid = new Grid();

                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var avatar = new Border
                    {
                        Width = 50,
                        Height = 50,
                        CornerRadius = new CornerRadius(25),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                        Margin = new Thickness(0, 0, 15, 0)
                    };
                    avatar.Child = new TextBlock
                    {
                        Text = $"{(student.Имя?.Length > 0 ? student.Имя[0] : '?')}{(student.Фамилия?.Length > 0 ? student.Фамилия[0] : '?')}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(avatar, 0);
                    grid.Children.Add(avatar);

                    var infoStack = new StackPanel();
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = student.ПолноеИмя,
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = $"Уровень: {student.Уровень ?? "Не указан"}",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"))
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = student.Email,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                        FontSize = 12
                    });

                    Grid.SetColumn(infoStack, 1);
                    grid.Children.Add(infoStack);

                    card.Child = grid;
                    ContentPanel.Children.Add(card);
                }
            }
            else
            {
                ContentPanel.Children.Add(new Border
                {
                    Style = (Style)FindResource("CardStyle"),
                    Child = new TextBlock
                    {
                        Text = "Список учеников пуст",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(50)
                    }
                });
            }
        }

        private void ShowMaterialsContent()
        {
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = "Учебные материалы",
                Style = (Style)FindResource("SectionHeaderStyle")
            });

            if (_materials != null && _materials.Count > 0)
            {
                foreach (var material in _materials)
                {
                    var card = new Border { Style = (Style)FindResource("MaterialCardStyle") };
                    var grid = new Grid();

                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var iconBlock = new TextBlock
                    {
                        Text = material.Иконка,
                        FontSize = 24,
                        Margin = new Thickness(0, 0, 15, 0)
                    };
                    Grid.SetColumn(iconBlock, 0);
                    grid.Children.Add(iconBlock);

                    var infoStack = new StackPanel();
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = material.Название,
                        FontWeight = FontWeights.Bold
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = material.Предмет ?? "Без предмета",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                        FontSize = 12
                    });

                    if (!string.IsNullOrEmpty(material.Описание))
                    {
                        infoStack.Children.Add(new TextBlock
                        {
                            Text = material.Описание.Length > 50 ?
                                   material.Описание.Substring(0, 47) + "..." :
                                   material.Описание,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6")),
                            FontSize = 11,
                            Margin = new Thickness(0, 5, 0, 0)
                        });
                    }

                    Grid.SetColumn(infoStack, 1);
                    grid.Children.Add(infoStack);

                    var typeBlock = new TextBlock
                    {
                        Text = material.Тип,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(typeBlock, 2);
                    grid.Children.Add(typeBlock);

                    card.Child = grid;
                    ContentPanel.Children.Add(card);
                }
            }
            else
            {
                ContentPanel.Children.Add(new Border
                {
                    Style = (Style)FindResource("CardStyle"),
                    Child = new TextBlock
                    {
                        Text = "Учебные материалы отсутствуют",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(50)
                    }
                });
            }
        }

        private void ShowHistoryContent()
        {
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = "История занятий",
                Style = (Style)FindResource("SectionHeaderStyle")
            });

            if (_history != null && _history.Count > 0)
            {
                foreach (var record in _history)
                {
                    var card = new Border { Style = (Style)FindResource("CardStyle") };
                    var stack = new StackPanel();

                    var headerGrid = new Grid();
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    headerGrid.Children.Add(new TextBlock
                    {
                        Text = $"{record.Дата} • {record.Время}",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"))
                    });

                    headerGrid.Children.Add(new TextBlock
                    {
                        Text = $"{record.СтоимостьЗанятия} ₽",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60")),
                        HorizontalAlignment = HorizontalAlignment.Right
                    });

                    stack.Children.Add(headerGrid);
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"{record.ФИО_ученика} • {record.Предмет}",
                        Margin = new Thickness(0, 5, 0, 5),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E"))
                    });

                    if (!string.IsNullOrEmpty(record.Тема))
                    {
                        stack.Children.Add(new TextBlock
                        {
                            Text = $"Тема: {record.Тема}",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                            FontSize = 12
                        });
                    }

                    if (!string.IsNullOrEmpty(record.ДомашнееЗадание))
                    {
                        stack.Children.Add(new TextBlock
                        {
                            Text = $"Д/з: {record.ДомашнееЗадание}",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB")),
                            FontSize = 12,
                            Margin = new Thickness(0, 5, 0, 0)
                        });
                    }

                    card.Child = stack;
                    ContentPanel.Children.Add(card);
                }
            }
            else
            {
                ContentPanel.Children.Add(new Border
                {
                    Style = (Style)FindResource("CardStyle"),
                    Child = new TextBlock
                    {
                        Text = "История занятий пуста",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(50)
                    }
                });
            }
        }

        private void ShowProfileContent()
        {
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = "Профиль и настройки",
                Style = (Style)FindResource("SectionHeaderStyle")
            });

            if (_currentTutor != null)
            {
                var profileCard = new Border { Style = (Style)FindResource("CardStyle") };
                var stack = new StackPanel();

                stack.Children.Add(CreateProfileField("Логин", _currentTutor.Логин));
                stack.Children.Add(CreateProfileField("Email", _currentTutor.Email));
                stack.Children.Add(CreateProfileField("Телефон", _currentTutor.Телефон ?? "Не указан"));
                stack.Children.Add(CreateProfileField("Предметы", _currentTutor.Предметы));
                stack.Children.Add(CreateProfileField("Рейтинг", _currentTutor.Рейтинг?.ToString() ?? "0"));
                stack.Children.Add(CreateProfileField("Стоимость часа", $"{_currentTutor.СтоимостьЧаса} руб."));

                profileCard.Child = stack;
                ContentPanel.Children.Add(profileCard);
            }
        }

        private Border CreateProfileField(string label, string value)
        {
            var border = new Border { Margin = new Thickness(0, 0, 0, 15) };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var labelBlock = new TextBlock
            {
                Text = label + ":",
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            var valueBlock = new TextBlock
            {
                Text = value,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(valueBlock, 2);
            grid.Children.Add(valueBlock);

            border.Child = grid;
            return border;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }
}