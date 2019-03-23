using System.Collections.Generic;
using System.IO;

namespace DNACoder.Core
{
    /// <summary>
    /// Поток, представляющий данные в троичной кодировке ДНК
    /// </summary>
    public class Encoded3Stream : EncodedStream
    {
        /// <summary>
        /// Кодер для троичной кодировки ДНК
        /// </summary>
        private class Code3Coder : CoderBase
        {
            /// <summary>
            /// Символы, используемые в данной кодировке
            /// </summary>
            static Dictionary<int, char> Chars3CC = new Dictionary<int, char>
            {
                [0] = 'A',
                [1] = 'C',
                [2] = 'G'
            };

            /// <summary>
            /// Преобразует число в строку с использованием символов 3СС
            /// </summary>
            /// <param name="value">Значение для преобразования</param>
            /// <returns></returns>
            static string ConvertCharTo3CC(int value)
            {
                Stack<char> result = new Stack<char>(capacity: 6);

                int current = value;

                for (int i = 0; i < 6; i++)
                {
                    result.Push(Chars3CC[current % 3]);
                    current /= 3;
                }

                return new string(result.ToArray());
            }

            /// <summary>
            /// Конструктор по умолчанию
            /// </summary>
            public Code3Coder()
            {
                BuildAlphabet(ConvertCharTo3CC);
            }
        }

        /// <summary>
        /// Константа заголовка потока
        /// </summary>
        public const string HeaderString = "ENC3_";

        /// <summary>
        /// Кодер, обеспечивающий работу данного потока
        /// </summary>
        public override ICoder Coder { get; } = new Code3Coder();

        /// <summary>
        /// Заголовок потока
        /// </summary>
        public override string Header => HeaderString;

        /// <summary>
        /// Конструктор, загружающий файл по указанному пути
        /// </summary>
        /// <param name="path">Путь для загрузки файла</param>
        /// <param name="mode">Режим открытия файла</param>
        public Encoded3Stream(string path, FileMode mode) : base(path, mode)
        {

        }
    }
}
