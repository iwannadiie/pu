using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class Занятие
    {
        public int ID_занятия { get; set; }
        public int ID_слота { get; set; }
        public int ID_ученика { get; set; }
        public int ID_репетитора { get; set; }
        public string Тема { get; set; }
        public string Статус { get; set; }
        public DateTime ДатаСоздания { get; set; }

        // Навигационные свойства
        public Ученик Ученик { get; set; }
        public Репетитор Репетитор { get; set; }
        public СлотРасписания Слот { get; set; }

        public DateTime ДатаЗанятия => Слот?.Дата ?? DateTime.MinValue;
        public string Время => Слот?.Время ?? "";
        public string УченикФИО => Ученик?.ПолноеИмя ?? "Не указан";

        public string СтатусЦвет
        {
            get
            {
                switch (Статус)
                {
                    case "Запланировано":
                        return "MediumOrchid";
                    case "Проведено":
                        return "Green";
                    case "Отменено":
                        return "Red";
                    case "Перенесено":
                        return "Orange";
                    default:
                        return "Gray";
                }
            }
        }
    }
}
