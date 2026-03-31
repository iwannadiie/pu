using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2
{
    public class Ученик
    {
        public int ID_ученика { get; set; }
        public string Логин { get; set; }
        public string Email { get; set; }
        public string Телефон { get; set; }
        public string Пароль { get; set; }
        public string Имя { get; set; }
        public string Фамилия { get; set; }
        public string Уровень { get; set; }

        public string ПолноеИмя => $"{Имя} {Фамилия}";
    }
}
