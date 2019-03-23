using System.Collections.Generic;
using System.IO;

namespace DNACoder.Core
{
    /// <summary>
    /// Поток, представляющий данные в четверичной кодировке ДНК
    /// </summary>
    public class Encoded4Stream : EncodedStream
    {
        /// <summary>
        /// Кодер для четверичной кодировки ДНК
        /// </summary>
        private class Coder4Code : CoderBase
        {
            /// <summary>
            /// Символы, используемые в данной кодировке
            /// </summary>
            static Dictionary<int, char> Chars4CC = new Dictionary<int, char>
            {
                [0] = 'A',
                [64] = 'C',
                [128] = 'G',
                [192] = 'T'
            };


            /// <summary>
            /// Преобразует число в строку с использованием символов 4СС
            /// </summary>
            /// <param name="value">Значение для преобразования</param>
            /// <returns></returns>
            static string ConvertCharTo4CC(int value)
            {
                const int mask = 0xC0;
                var c = (byte)value;

                var c1 = Chars4CC[c & mask];
                var c2 = Chars4CC[(c << 2) & mask];
                var c3 = Chars4CC[(c << 4) & mask];
                var c4 = Chars4CC[(c << 6) & mask];

                return $"{c1}{c2}{c3}{c4}";
            }

            /// <summary>
            /// Конструктор по умолчанию
            /// </summary>
            public Coder4Code()
            {
                BuildAlphabet(ConvertCharTo4CC);
            }
        }

        /// <summary>
        /// Константа заголовка потока
        /// </summary>
        public const string HeaderString = "ENC4_";

        /// <summary>
        /// Кодер, обеспечивающий работу данного потока
        /// </summary>
        public override ICoder Coder { get; } = new Coder4Code();

        /// <summary>
        /// Заголовок потока
        /// </summary>
        public override string Header => HeaderString;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        public Encoded4Stream(string path, FileMode mode) : base(path, mode)
        {
        }
    }
}
