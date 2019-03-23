using System;
using System.IO;
using System.Text;

namespace DNACoder.Core
{
    /// <summary>
    /// Закодированный файловый поток
    /// </summary>
    public abstract class EncodedStream : FileStream
    {
        /// <summary>
        /// Длина заголовка для закодированного потока
        /// </summary>
        public const int TypicalHeaderLength = 5;

        /// <summary>
        /// Кодер, обеспечивающий работу данного потока
        /// </summary>
        public abstract ICoder Coder { get; }

        /// <summary>
        /// Кодировка символов, применяющаяся для хранеия данных
        /// </summary>
        public virtual Encoding Encoding { get; } = Encoding.ASCII;

        /// <summary>
        /// Количество символов, которые хранятся для каждого исходного байта информации
        /// </summary>
        public virtual int CharsPerByte => Encoding.GetByteCount(" ") * Coder.WordLength;
        
        /// <summary>
        /// Заголовок потока
        /// </summary>
        public abstract string Header { get; }

        /// <summary>
        /// Длина заголовка в байтах
        /// </summary>
        public virtual int HeaderLength => Header.Length;

        #region [Переопределенные члены]

        /// <summary>
        /// Длина потока без учета заголовка
        /// </summary>
        public override long Length => (base.Length - HeaderLength) / CharsPerByte;

        /// <summary>
        /// Позиция в потоке без учета заголовка
        /// </summary>
        public override long Position
        {
            get => (base.Position - HeaderLength) / CharsPerByte;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// Перейти к позиции в потоке без учета заголовка
        /// </summary>
        /// <param name="offset">Смещение относительно отправной точки</param>
        /// <param name="origin">Тип отправной точки</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
            => base.Seek((origin == SeekOrigin.Begin ? HeaderLength : 0)
                +  offset * CharsPerByte, origin);

        /// <summary>
        /// Кодирует байт при помощи кодера в правильной кодировке
        /// </summary>
        /// <param name="value">Байт, который требуется закодировать</param>
        /// <returns>Массив байтов, полученных из приведения к заданной кодировке</returns>
        private byte[] Encode(byte value) 
            => Encoding.GetBytes(Coder.Encode(value));

        /// <summary>
        /// Записывает один байт в правильной кодировке в поток
        /// </summary>
        /// <param name="value">Байт, который требуется записать</param>
        public override void WriteByte(byte value)
        {
            var bytes = Encode(value);

            for (int i = 0; i < bytes.Length; i++)
            {
                base.WriteByte(bytes[i]);
            }
        }

        /// <summary>
        /// Производит декодирование массива байтов при помощи кодера
        /// </summary>
        /// <param name="encoded">Массив байтов для декодирования</param>
        /// <param name="index">Индекс начала</param>
        /// <param name="charCount">Количество символов в одном байте</param>
        /// <returns>Декодированный байт</returns>
        private byte Decode(byte[] encoded, int index, int charCount) 
            => Coder.Decode(Encoding.GetString(encoded, index * charCount, charCount));
        
        /// <summary>
        /// Читает следующий байт в потоке
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            var charCount = CharsPerByte;
            var bytes = new byte[charCount];

            for (int i = 0; i < charCount; i++)
            {
                bytes[i] = (byte)base.ReadByte();
            }

            return Decode(bytes, 0, charCount);
        }

        /// <summary>
        /// Производит чтение массива байтов вправильной кодировке
        /// </summary>
        /// <param name="array">Массив байтов, в который необходимо записать прочитанные байты</param>
        /// <param name="offset">Смещение в массиве, с которого начнется запись данных</param>
        /// <param name="count">Количество байтов, которые требуется записать, начиная со смещения</param>
        /// <returns>Количество считанных байтов</returns>
        public override int Read(byte[] array, int offset, int count)
        {
            var charCount = CharsPerByte;
            var len = array.Length * charCount;
            var bytes = new byte[len];

            count = base.Read(bytes, offset * charCount, len) / charCount;

            for (int i = offset; i < count; i++)
            {
                array[i] = Decode(bytes, i, charCount);
            }

            return count;
        }

        /// <summary>
        /// Производит запись массива байтов в правильной кодировке в поток
        /// </summary>
        /// <param name="array">Массив байтов, которые необходимо записать</param>
        /// <param name="offset">Смещение в массиве, с которого начнется запись данных</param>
        /// <param name="count">Количество байтов, которые требуется записать, начиная со смещения</param>
        public override void Write(byte[] array, int offset, int count)
        {
            if (offset + count > array.Length)
                throw new ArgumentException("offset + count is out of bounds of array");

            var charCount = CharsPerByte;
            var bytes = new byte[count * charCount];

            for (int i = offset; i < offset + count; i++)
            {
                Encode(array[i]).CopyTo(bytes, (i - offset) * charCount);
            }

            base.Write(bytes, 0, bytes.Length);
        }

        #endregion

        /// <summary>
        /// Конструктор потока по умолчанию
        /// </summary>
        /// <param name="path">Путь для открытия файла</param>
        /// <param name="mode">Режим открытия файлового потока</param>
        /// <returns></returns>
        protected EncodedStream(string path, FileMode mode) : base(path, mode)
        {
            // инициализация заголовка
            if (base.Length == 0)
            {
                var headerBytes = Encoding.GetBytes(Header);

                base.Seek(0, SeekOrigin.Begin);
                base.Write(headerBytes, 0, headerBytes.Length);
            }
            else
            {
                base.Seek(HeaderLength, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Открывает закодированный файловый поток по указанному пути
        /// </summary>
        /// <param name="path">Путь для открытия файла</param>
        /// <param name="mode">Режим открытия файлового потока</param>
        /// <returns></returns>
        public static EncodedStream Open(string path, FileMode mode)
        {
            var header = string.Empty;

            using (var file = new BinaryReader(File.OpenRead(path)))
            {
                header = new string(file.ReadChars(TypicalHeaderLength));
            }

            switch (header)
            {
                case Encoded3Stream.HeaderString:
                    {
                        return new Encoded3Stream(path, mode);
                    }
                case Encoded4Stream.HeaderString:
                    {
                        return new Encoded4Stream(path, mode);
                    }
                default:
                    {
                        throw new InvalidDataException($"Поток с заголовком '{header}' не может быть прочитан");
                    }
            }
        }
    }
}
