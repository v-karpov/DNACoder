using System;
using System.Collections.Generic;
using System.Linq;

namespace DNACoder.Core
{
    /// <summary>
    /// Базовый класс кодера для различных кодировок
    /// </summary>
    public abstract class CoderBase : ICoder
    {
        /// <summary>
        /// Все возможные наборы кодов для данного кодера
        /// </summary>
        public Dictionary<byte, string> Alphabet { get; protected set; }

        /// <summary>
        /// Обращенный набор кодов для расшифровки 
        /// </summary>
        public Dictionary<string, byte> ReverseAlphabet { get; protected set; }

        /// <summary>
        /// Длина одного кодового слова
        /// </summary>
        public int WordLength { get; protected set; }

        /// <summary>
        /// Строит прямой и обратный алфавиты на основе функции получения кода для символа
        /// </summary>
        /// <param name="charConverter">Функция получения кода для символа</param>
        protected void BuildAlphabet(Func<int, string> charConverter)
        {
            var codeWords = Enumerable.Range(0, 256)
                .Select((x, i) => new { Key = i, Value = charConverter(x) });

            Alphabet = codeWords
                .ToDictionary(x => (byte)x.Key, y => y.Value);

            ReverseAlphabet = Alphabet
                .ToDictionary(x => x.Value, y => y.Key);

            WordLength = codeWords.First().Value.Length;
        }

        /// <summary>
        /// Преобразует кодовое слово в байт
        /// </summary>
        /// <param name="codeWord">Кодовое слово для преобразования</param>
        /// <returns></returns>
        public byte Decode(string codeWord) => ReverseAlphabet[codeWord];

        /// <summary>
        /// Преобразует байт в кодовое слово с использованием словаря
        /// </summary>
        /// <param name="value">Байт для преобразвоания в кодовое слово</param>
        /// <returns></returns>
        public string Encode(byte value) => Alphabet[value];
    }
}
