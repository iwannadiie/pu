using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class УчебныйМатериал
    {
        public int ID_материала { get; set; }
        public string Название { get; set; }
        public string Тип { get; set; }
        public string ФайлПуть { get; set; }
        public string Предмет { get; set; }
        public string Описание { get; set; }
        public string Теги { get; set; }
        public DateTime ДатаЗагрузки { get; set; }

        // Навигационное свойство
        public Репетитор Репетитор { get; set; }

        public string Иконка
        {
            get
            {
                string типВерхний = Тип?.ToUpper() ?? "";

                switch (типВерхний)
                {
                    case "PDF":
                        return "PDF";
                    case "DOC":
                        return "DOC";
                    case "VIDEO":
                        return "VIDEO";
                    case "LINK":
                        return "LINK";
                    case "PRESENTATION":
                        return "PRES";
                    default:
                        return "FILE";
                }
            }
        }

        public string КороткоеОписание => Описание?.Length > 50
            ? Описание.Substring(0, 47) + "..."
            : Описание;

        public string ТегиСписок => Теги?.Replace(",", " • ");
    }
}
