using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class TutorDbContext
    {
        private readonly string _connectionString;

        public TutorDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<Репетитор> GetTutorByIdAsync(int id)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Репетиторы WHERE ID_репетитора = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Репетитор
                            {
                                ID_репетитора = reader.GetInt32(0),
                                Логин = reader.GetString(1),
                                Email = reader.GetString(2),
                                Телефон = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Пароль = reader.GetString(4),
                                Имя = reader.GetString(5),
                                Фамилия = reader.GetString(6),
                                Предметы = reader.GetString(7),
                                Рейтинг = reader.IsDBNull(8) ? null : (decimal?)reader.GetDecimal(8),
                                СтоимостьЧаса = reader.GetDecimal(9)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<List<Ученик>> GetAllStudentsAsync()
        {
            var ученики = new List<Ученик>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Ученики ORDER BY Фамилия, Имя";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ученики.Add(new Ученик
                        {
                            ID_ученика = reader.GetInt32(0),
                            Логин = reader.GetString(1),
                            Email = reader.GetString(2),
                            Телефон = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Пароль = reader.GetString(4),
                            Имя = reader.GetString(5),
                            Фамилия = reader.GetString(6),
                            Уровень = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }

            return ученики;
        }

        public async Task<List<Занятие>> GetLessonsForMonthAsync(int tutorId, DateTime startDate, DateTime endDate)
        {
            var занятия = new List<Занятие>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT з.*, с.*, у.* 
                    FROM Занятия з
                    JOIN СлотыРасписания с ON з.ID_слота = с.ID_слота
                    JOIN Ученики у ON з.ID_ученика = у.ID_ученика
                    WHERE з.ID_репетитора = @tutorId 
                    AND CAST(с.ДатаВремяНачала AS DATE) BETWEEN @startDate AND @endDate
                    ORDER BY с.ДатаВремяНачала";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tutorId", tutorId);
                    command.Parameters.AddWithValue("@startDate", startDate.Date);
                    command.Parameters.AddWithValue("@endDate", endDate.Date);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var занятие = new Занятие
                            {
                                ID_занятия = reader.GetInt32(0),
                                ID_слота = reader.GetInt32(1),
                                ID_ученика = reader.GetInt32(2),
                                ID_репетитора = reader.GetInt32(3),
                                Тема = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Статус = reader.GetString(5),
                                ДатаСоздания = reader.GetDateTime(6),
                                Слот = new СлотРасписания
                                {
                                    ID_слота = reader.GetInt32(7),
                                    ID_репетитора = reader.GetInt32(8),
                                    ДатаВремяНачала = reader.GetDateTime(9),
                                    ДатаВремяОкончания = reader.GetDateTime(10),
                                    СтатусСлота = reader.GetString(11),
                                    ID_ученика = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                                    ДатаБронирования = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13)
                                },
                                Ученик = new Ученик
                                {
                                    ID_ученика = reader.GetInt32(14),
                                    Логин = reader.GetString(15),
                                    Email = reader.GetString(16),
                                    Телефон = reader.IsDBNull(17) ? null : reader.GetString(17),
                                    Пароль = reader.GetString(18),
                                    Имя = reader.GetString(19),
                                    Фамилия = reader.GetString(20),
                                    Уровень = reader.IsDBNull(21) ? null : reader.GetString(21)
                                }
                            };

                            занятия.Add(занятие);
                        }
                    }
                }
            }

            return занятия;
        }

        public async Task<List<Занятие>> GetLessonsForDateAsync(int tutorId, DateTime date)
        {
            var занятия = new List<Занятие>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT з.*, с.*, у.* 
                    FROM Занятия з
                    JOIN СлотыРасписания с ON з.ID_слота = с.ID_слота
                    JOIN Ученики у ON з.ID_ученика = у.ID_ученика
                    WHERE з.ID_репетитора = @tutorId 
                    AND CAST(с.ДатаВремяНачала AS DATE) = @date
                    ORDER BY с.ДатаВремяНачала";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tutorId", tutorId);
                    command.Parameters.AddWithValue("@date", date.Date);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var занятие = new Занятие
                            {
                                ID_занятия = reader.GetInt32(0),
                                ID_слота = reader.GetInt32(1),
                                ID_ученика = reader.GetInt32(2),
                                ID_репетитора = reader.GetInt32(3),
                                Тема = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Статус = reader.GetString(5),
                                ДатаСоздания = reader.GetDateTime(6),
                                Слот = new СлотРасписания
                                {
                                    ID_слота = reader.GetInt32(7),
                                    ID_репетитора = reader.GetInt32(8),
                                    ДатаВремяНачала = reader.GetDateTime(9),
                                    ДатаВремяОкончания = reader.GetDateTime(10),
                                    СтатусСлота = reader.GetString(11),
                                    ID_ученика = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                                    ДатаБронирования = reader.IsDBNull(13) ? (DateTime?)null : reader.GetDateTime(13)
                                },
                                Ученик = new Ученик
                                {
                                    ID_ученика = reader.GetInt32(14),
                                    Логин = reader.GetString(15),
                                    Email = reader.GetString(16),
                                    Телефон = reader.IsDBNull(17) ? null : reader.GetString(17),
                                    Пароль = reader.GetString(18),
                                    Имя = reader.GetString(19),
                                    Фамилия = reader.GetString(20),
                                    Уровень = reader.IsDBNull(21) ? null : reader.GetString(21)
                                }
                            };

                            занятия.Add(занятие);
                        }
                    }
                }
            }

            return занятия;
        }

        public async Task<List<ИсторияСделки>> GetLessonHistoryAsync(int tutorId)
        {
            var история = new List<ИсторияСделки>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT * FROM ИсторияСделок 
                    WHERE ID_репетитора = @tutorId 
                    ORDER BY ДатаПроведения DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tutorId", tutorId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            история.Add(new ИсторияСделки
                            {
                                ID_записи = reader.GetInt32(0),
                                ID_занятия = reader.GetInt32(1),
                                ID_репетитора = reader.GetInt32(2),
                                ID_ученика = reader.GetInt32(3),
                                ФИО_ученика = reader.GetString(4),
                                ВремяНачала = reader.GetTimeSpan(5),
                                ВремяОкончания = reader.GetTimeSpan(6),
                                Предмет = reader.GetString(7),
                                Тема = reader.IsDBNull(8) ? null : reader.GetString(8),
                                СтоимостьЗанятия = reader.GetDecimal(9),
                                ДомашнееЗадание = reader.IsDBNull(10) ? null : reader.GetString(10),
                                ДатаПроведения = reader.GetDateTime(11)
                            });
                        }
                    }
                }
            }

            return история;
        }

        public async Task<List<УчебныйМатериал>> GetStudyMaterialsAsync()
        {
            var материалы = new List<УчебныйМатериал>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM УчебныеМатериалы ORDER BY ДатаЗагрузки DESC";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        материалы.Add(new УчебныйМатериал
                        {
                            ID_материала = reader.GetInt32(0),
                            Название = reader.GetString(1),
                            Тип = reader.GetString(2),
                            ФайлПуть = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Предмет = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Описание = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Теги = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ДатаЗагрузки = reader.GetDateTime(7)
                        });
                    }
                }
            }

            return материалы;
        }

        public async Task UpdatePastLessonsAsync()
        {
            var pastLessons = new List<int>();

            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT з.ID_занятия 
                    FROM Занятия з
                    JOIN СлотыРасписания с ON з.ID_слота = с.ID_слота
                    WHERE з.Статус = 'Запланировано' 
                    AND с.ДатаВремяОкончания < GETDATE()";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pastLessons.Add(reader.GetInt32(0));
                    }
                }
            }

            foreach (var id in pastLessons)
            {
                await UpdateLessonStatusAsync(id, "Проведено");
            }
        }

        public async Task UpdateLessonStatusAsync(int lessonId, string newStatus)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var updateQuery = "UPDATE Занятия SET Статус = @status WHERE ID_занятия = @id";
                        using (var command = new SqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@status", newStatus);
                            command.Parameters.AddWithValue("@id", lessonId);
                            await command.ExecuteNonQueryAsync();
                        }

                        if (newStatus == "Проведено")
                        {
                            await AddLessonToHistoryAsync(lessonId, connection, transaction);
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Ошибка при обновлении статуса: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        private async Task AddLessonToHistoryAsync(int lessonId, SqlConnection connection, SqlTransaction transaction)
        {
            var selectQuery = @"
                SELECT 
                    з.ID_занятия,
                    з.ID_репетитора,
                    з.ID_ученика,
                    у.Имя + ' ' + у.Фамилия as ФИО_ученика,
                    CAST(с.ДатаВремяНачала AS TIME) as ВремяНачала,
                    CAST(с.ДатаВремяОкончания AS TIME) as ВремяОкончания,
                    р.Предметы as Предмет,
                    з.Тема,
                    р.СтоимостьЧаса * (DATEDIFF(MINUTE, с.ДатаВремяНачала, с.ДатаВремяОкончания) / 60.0) as СтоимостьЗанятия,
                    GETDATE() as ДатаПроведения
                FROM Занятия з
                JOIN СлотыРасписания с ON з.ID_слота = с.ID_слота
                JOIN Репетиторы р ON з.ID_репетитора = р.ID_репетитора
                JOIN Ученики у ON з.ID_ученика = у.ID_ученика
                WHERE з.ID_занятия = @id";

            using (var selectCommand = new SqlCommand(selectQuery, connection, transaction))
            {
                selectCommand.Parameters.AddWithValue("@id", lessonId);

                int id = 0, repId = 0, uchId = 0;
                string fio = "", predmet = "", tema = null;
                TimeSpan timeStart = TimeSpan.Zero, timeEnd = TimeSpan.Zero;
                decimal cost = 0;
                DateTime date = DateTime.Now;

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        id = reader.GetInt32(0);
                        repId = reader.GetInt32(1);
                        uchId = reader.GetInt32(2);
                        fio = reader.GetString(3);
                        timeStart = reader.GetTimeSpan(4);
                        timeEnd = reader.GetTimeSpan(5);
                        predmet = reader.GetString(6);
                        tema = reader.IsDBNull(7) ? null : reader.GetString(7);
                        cost = reader.GetDecimal(8);
                        date = reader.GetDateTime(9);
                    }
                }

                var insertQuery = @"
                    INSERT INTO ИсторияСделок (
                        ID_занятия, ID_репетитора, ID_ученика, ФИО_ученика,
                        ВремяНачала, ВремяОкончания, Предмет, Тема, 
                        СтоимостьЗанятия, ДатаПроведения
                    ) VALUES (
                        @id, @repId, @uchId, @fio,
                        @timeStart, @timeEnd, @predmet, @tema,
                        @cost, @date
                    )";

                using (var insertCommand = new SqlCommand(insertQuery, connection, transaction))
                {
                    insertCommand.Parameters.AddWithValue("@id", id);
                    insertCommand.Parameters.AddWithValue("@repId", repId);
                    insertCommand.Parameters.AddWithValue("@uchId", uchId);
                    insertCommand.Parameters.AddWithValue("@fio", fio);
                    insertCommand.Parameters.AddWithValue("@timeStart", timeStart);
                    insertCommand.Parameters.AddWithValue("@timeEnd", timeEnd);
                    insertCommand.Parameters.AddWithValue("@predmet", predmet);
                    insertCommand.Parameters.AddWithValue("@tema", (object)tema ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@cost", cost);
                    insertCommand.Parameters.AddWithValue("@date", date);

                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}