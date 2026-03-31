using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class ИсторияСделки
    {
        public int ID_записи { get; set; }
        public int ID_занятия { get; set; }
        public int ID_репетитора { get; set; }
        public int ID_ученика { get; set; }
        public string ФИО_ученика { get; set; }
        public TimeSpan ВремяНачала { get; set; }
        public TimeSpan ВремяОкончания { get; set; }
        public string Предмет { get; set; }
        public string Тема { get; set; }
        public decimal СтоимостьЗанятия { get; set; }
        public string ДомашнееЗадание { get; set; }
        public DateTime ДатаПроведения { get; set; }

        public string Время => $"{ВремяНачала:hh\\:mm} - {ВремяОкончания:hh\\:mm}";
        public string Дата => ДатаПроведения.ToString("dd.MM.yyyy");
    }
}
