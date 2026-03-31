using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class Репетитор
    {
        public int ID_репетитора { get; set; }
        public string Логин { get; set; }
        public string Email { get; set; }
        public string Телефон { get; set; }
        public string Пароль { get; set; }
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public string Предметы { get; set; }
        public decimal? Рейтинг { get; set; }
        public decimal СтоимостьЧаса { get; set; }

        public string ПолноеИмя => $"{Имя} {Фамилия}";
        public string Инициалы => $"{(Имя?.Length > 0 ? Имя[0] : '?')}{(Фамилия?.Length > 0 ? Фамилия[0] : '?')}";
    }
}
