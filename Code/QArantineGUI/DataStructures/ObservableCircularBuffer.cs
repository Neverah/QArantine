using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace QArantine.Code.QArantineGUI.DataStructures
{
    public class ObservableCircularBuffer<T> : ObservableCollection<T>
    {
        private readonly T[] _buffer;
        private int _start;
        private int _count;

        public new int Count => _count;
        public int Capacity { get; }

        public ObservableCircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("ObservableCircularBuffer Capacity must be greater than 0", nameof(capacity));

            Capacity = capacity;
            _buffer = new T[capacity];
            _start = 0;
            _count = 0;
        }

        public new void Add(T item)
        {
            if (_count < Capacity)
            {
                // Agregar un nuevo elemento sin sobrescribir
                _buffer[(_start + _count) % Capacity] = item;
                _count++;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _count - 1));
            }
            else
            {
                // Calcular el número de elementos a eliminar (10% de la capacidad)
                int itemsToRemove = Math.Max(1, Capacity / 10);

                // Eliminar los elementos más antiguos
                for (int i = 0; i < itemsToRemove; i++)
                {
                    T? removedItem = _buffer[_start];
                    _buffer[_start] = default!;
                    _start = (_start + 1) % Capacity;
                    _count--;

                    // Notificar que el elemento fue eliminado
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, 0));
                }

                // Agregar el nuevo elemento
                _buffer[(_start + _count) % Capacity] = item;
                _count++;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _count - 1));
            }

            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        }

        public new void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _start = 0;
            _count = 0;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        }

        public new T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _buffer[(_start + index) % Capacity];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var oldItem = _buffer[(_start + index) % Capacity];
                _buffer[(_start + index) % Capacity] = value;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (index < 0 || index > _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_count == Capacity)
                throw new InvalidOperationException("Cannot insert into a full buffer.");

            // Desplazar elementos hacia la derecha para hacer espacio
            for (int i = _count; i > index; i--)
            {
                _buffer[(_start + i) % Capacity] = _buffer[(_start + i - 1) % Capacity];
            }

            _buffer[(_start + index) % Capacity] = item;
            _count++;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        }

        protected override void RemoveItem(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var removedItem = _buffer[(_start + index) % Capacity];

            // Desplazar elementos hacia la izquierda para llenar el hueco
            for (int i = index; i < _count - 1; i++)
            {
                _buffer[(_start + i) % Capacity] = _buffer[(_start + i + 1) % Capacity];
            }

            _buffer[(_start + _count - 1) % Capacity] = default!;
            _count--;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        }

        protected override void SetItem(int index, T item)
        {
            this[index] = item;
        }

        protected override void ClearItems()
        {
            Clear();
        }
    }
}