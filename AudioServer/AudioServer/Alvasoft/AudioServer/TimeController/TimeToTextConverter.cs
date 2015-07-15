using System;
using System.Collections.Generic;

namespace Alvasoft.AudioServer.TimesController
{
    /// <summary>
    /// Конвертер времени в текстовое представление.
    /// </summary>
    public static class TimeToTextConverter
    {
        private static List<string> digits = new List<string> {
            "ноль", "один", "два", "три", 
            "четыре", "пять", "шесть", "семь", 
            "восемь", "девять", "десять", "одиннадцать", 
            "двенадцать", "тринадцать", "четырнадцать", 
            "пятнадцать", "шестнадцать", "семнадцать", 
            "восемнадцать", "девятнадцать"
        };

        private static List<string> tens = new List<string> {
            "десять", "двадцать", "тридцать", "сорок", "пятьдесят"
        };

        /// <summary>
        /// Конвертирует время в числовое представление.
        /// </summary>
        /// <param name="time">Время.</param>
        /// <returns>Строка в числовом представлении. Например: "11 часов, 23 минуты"</returns>
        public static string ConvertDig(DateTime time)
        {
            var hours = HourToText(time.Hour);
            var minutes = MinuteToText(time.Minute);                       

            return time.Hour + " " + hours + ", " + time.Minute + " " + minutes;
        }

        /// <summary>
        /// Конвертирует время в строковое представление.
        /// </summary>
        /// <param name="time">Время.</param>
        /// <returns>Время в строковом представлении. Например: "Одиннадцать часов, двадцать три минуты"</returns>
        public static string Convert(DateTime time)
        {            
            var hour = time.Hour;
            var hourText = string.Empty;

            if (hour < 20)
            {
                hourText = digits[hour];
            }
            else
            {
                hourText = tens[hour/10 - 1];
                if (hour%10 > 0)
                {
                    hourText += " " + digits[hour%10];
                }                
            }

            var minute = time.Minute;

            if (minute == 0) {
                return hourText + " " + HourToText(time.Hour);
            }

            var minText = string.Empty;
            if (minute < 20)
            {
                if (minute != 1 && minute != 2)
                {
                    minText = digits[minute];
                }
                else
                {
                    minText = (minute == 1 ? "одна" : "две");
                }
            }
            else
            {
                minText = tens[minute/10 - 1];
                if (minute%10 > 0)
                {
                    if (minute%10 > 2)
                    {
                        minText += " " + digits[minute%10];
                    }
                    else
                    {
                        minText += " " + (minute%10 == 1 ? "одна" : "две");
                    }
                }
            }            

            return hourText + " " + HourToText(time.Hour) + ", " + minText + " " + MinuteToText(minute);
        }

        /// <summary>
        /// Возвращает правильное склонение для теукщего часа.
        /// </summary>
        /// <param name="hour">Час.</param>
        /// <returns>Строка, определяющая правильное склоление для текущего часа. Например: "час" или "часа", или "часов".</returns>
        private static string HourToText(int hour)
        {
            var result = string.Empty;

            if (hour == 0 || (hour >= 5 && hour <= 20))
            {
                result = "часов";
            }
            else if (hour == 1 || hour == 21)
            {
                result = "час";
            }
            else
            {
                result = "часа";
            }

            return result;
        }

        /// <summary>
        /// Возвращает правильное склонение слова "минуты" для минут.
        /// </summary>
        /// <param name="minute">Минута.</param>
        /// <returns>Например: "минут" или "минута", или "минуты".</returns>
        private static string MinuteToText(int minute)
        {
            var result = string.Empty;

            if (minute % 10 == 0 || (minute >= 5 && minute <= 20) || (minute % 10 >= 5))
            {
                result = "минут";
            }
            else if (minute % 10 == 1)
            {
                result = "минута";
            }
            else
            {
                result = "минуты";
            }

            return result;
        }
    }
}
