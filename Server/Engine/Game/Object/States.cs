using System;
using System.Collections;
using System.Collections.Generic;

namespace Server
{
	public interface IStates<T> : IEnumerable<T>
	{
		T[] Data { get; }

		int Length { get; }

		void Clear();
	}

	/*
	public abstract class BaseStates<T> : IEnumerable<T>
	{
		protected readonly T[] m_Data;

		public virtual int Length => m_Data.Length;

		public T this[int index] { get => Get(index); set => Set(index, value); }

		public BaseStates(int length)
		{
			m_Data = length > 0 ? new T[length] : Array.Empty<T>();
		}

		public BaseStates(int length, GenericReader reader)
			: this(length)
		{
			Deserialize(reader);
		}

		public override string ToString()
		{
			return "...";
		}

		public virtual void Clear()
		{
			Array.Clear(m_Data, 0, m_Data.Length);
		}

		protected virtual T Get(int index)
		{
			if (index >= 0 && index < m_Data.Length)
			{
				return m_Data[index];
			}

			return default;
		}

		protected virtual void Set(int index, T value)
		{
			if (index >= 0 && index < m_Data.Length)
			{
				m_Data[index] = value;
			}
		}

		public virtual IEnumerator<T> GetEnumerator()
		{
			foreach (var value in m_Data)
			{
				yield return value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(Length);

			for (var i = 0; i < Length; i++)
			{
				WriteData(writer, i, this[i]);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.ReadInt();

			var length = reader.ReadInt();

			for (var i = 0; i < length; i++)
			{
				if (i < Length)
				{
					this[i] = ReadData(reader, i);
				}
				else
				{
					_ = ReadData(reader, i);
				}
			}
		}

		protected virtual void WriteData(GenericWriter writer, int index, T value)
		{
		}

		protected virtual T ReadData(GenericReader reader, int index)
		{
			return default;
		}
	}
	*/

	public abstract class BaseStates<TKey, TVal> : IStates<TVal> where TKey : struct, Enum
	{
		protected static readonly TKey[] EnumValues = Enum.GetValues<TKey>();

		protected readonly TVal[] m_Data;

		TVal[] IStates<TVal>.Data => m_Data;

		public virtual int Length => m_Data.Length;

		public TVal this[TKey key] { get => Get(key); set => Set(key, value); }

		public BaseStates()
			: this(EnumValues.Length)
		{
		}

		protected BaseStates(int length)
		{
			m_Data = length > 0 ? new TVal[length] : Array.Empty<TVal>();
		}

		public BaseStates(GenericReader reader)
			: this(EnumValues.Length, reader)
		{
		}

		protected BaseStates(int length, GenericReader reader)
			: this(length)
		{
			Deserialize(reader);
		}

		public override string ToString()
		{
			return "...";
		}

		public virtual void SetAll(TVal value)
		{
			foreach (var o in EnumValues)
			{
				this[o] = value;
			}
		}

		public virtual void Clear()
		{
			Array.Clear(m_Data, 0, m_Data.Length);
		}

		protected virtual TVal Get(TKey key)
		{
			var index = Array.IndexOf(EnumValues, key);

			if (index >= 0 && index < m_Data.Length)
			{
				return m_Data[index];
			}

			return default;
		}

		protected virtual void Set(TKey key, TVal value)
		{
			var index = Array.IndexOf(EnumValues, key);

			if (index >= 0 && index < m_Data.Length)
			{
				m_Data[index] = value;
			}
		}

		public virtual IEnumerator<TVal> GetEnumerator()
		{
			foreach (var value in EnumValues)
			{
				yield return this[value];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(Length);

			for (var i = 0; i < Length; i++)
			{
				var key = EnumValues[i];

				writer.Write(key);

				WriteData(writer, key, this[key]);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.ReadInt();

			var length = reader.ReadInt();

			for (var i = 0; i < length; i++)
			{
				var key = reader.ReadEnum<TKey>();

				this[key] = ReadData(reader, key);
			}
		}

		protected abstract TVal ReadData(GenericReader reader, TKey key);
		protected abstract void WriteData(GenericWriter writer, TKey key, TVal value);
	}
}