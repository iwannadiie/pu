using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class СлотРасписания
    {
        public int ID_слота { get; set; }
        public int ID_репетитора { get; set; }
        public DateTime ДатаВремяНачала { get; set; }
        public DateTime ДатаВремяОкончания { get; set; }
        public string СтатусСлота { get; set; }
        public int? ID_ученика { get; set; }
        public DateTime? ДатаБронирования { get; set; }

        public DateTime Дата => ДатаВремяНачала.Date;
        public TimeSpan ВремяНачала => ДатаВремяНачала.TimeOfDay;
        public TimeSpan ВремяОкончания => ДатаВремяОкончания.TimeOfDay;
        public string Время => $"{ВремяНачала:hh\\:mm} - {ВремяОкончания:hh\\:mm}";
    }
}
