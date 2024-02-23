#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion

namespace Server
{
	public static class PropertyNotifier
	{
		public static event EventHandler<PropertyChangedEventArgs> PropertyChanged;

		public static void Notify(object sender, string propertyName, object oldValue, object newValue)
		{
			var type = sender.GetType();

			var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			Notify(sender, property, oldValue, newValue);
		}

		public static void Notify(object sender, PropertyInfo property, object oldValue, object newValue)
		{
			PropertyChanged?.Invoke(sender, new(property, oldValue, newValue));
		}
	}

	public class PropertyChangedEventArgs : EventArgs
	{
		public PropertyInfo Property { get; }

		public object OldValue { get; }
		public object NewValue { get; }

		public PropertyChangedEventArgs(PropertyInfo property, object oldValue, object newValue)
		{
			Property = property;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	[PropertyObject]
	public class TypeAmount : TypeEntry, IEquatable<TypeAmount>
	{
		protected int _Amount = 1;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Amount { get => _Amount; set => _Amount = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public override bool IsActive => base.IsActive && Amount != 0;

		public TypeAmount()
			: base()
		{
		}

		public TypeAmount(Type type)
			: base(type)
		{
		}

		public TypeAmount(Type type, int amount)
			: base(type)
		{
			_Amount = amount;
		}

		public TypeAmount(Type type, int amount, bool state)
			: base(type, state)
		{
			_Amount = amount;
		}

		public TypeAmount(Type type, int amount, bool state, bool inherit)
			: base(type, state, inherit)
		{
			_Amount = amount;
		}

		public TypeAmount(GenericReader reader)
			: base(reader)
		{
		}

		public override string ToString()
		{
			return $"{Type?.Name ?? "Null"}[{Amount}]";
		}

		public override int GetHashCode()
		{
			return Type?.GetHashCode() ?? 0;
		}

		public override bool Equals(object o)
		{
			return (o is TypeAmount e && Equals(e)) || base.Equals(o);
		}

		public override bool Equals(TypeEntry t)
		{
			return (t is TypeAmount e && Equals(e)) || base.Equals(t);
		}

		public virtual bool Equals(TypeAmount t)
		{
			return ReferenceEquals(this, t) || base.Equals(t);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);

			writer.WriteEncodedInt(_Amount);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadEncodedInt();

			_Amount = reader.ReadEncodedInt();
		}

		public static implicit operator int(TypeAmount t)
		{
			return t?.Amount ?? default;
		}
	}

	[PropertyObject]
	public class TypeAmounts : TypeList<TypeAmount>
	{
		public IEnumerable<int> Amounts => _Entries.Select(e => e.Amount);

		public override object this[Type type]
		{
			get => base[type];
			set
			{
				if (value is int a)
				{
					SetAmount(type, a);
				}
				else
				{
					base[type] = value;
				}
			}
		}

		public virtual int DefaultAmountMin => Int32.MinValue;
		public virtual int DefaultAmountMax => Int32.MaxValue;

		public int AmountMin { get; set; }
		public int AmountMax { get; set; }

		public TypeAmounts()
		{ }

		public TypeAmounts(params TypeAmount[] entries)
			: base(entries)
		{ }

		public TypeAmounts(IEnumerable<TypeAmount> entries)
			: base(entries)
		{ }

		public TypeAmounts(GenericReader reader)
			: base(reader)
		{ }

		protected override void OnInit()
		{
			base.OnInit();

			AmountMin = DefaultAmountMin;
			AmountMax = DefaultAmountMax;
		}

		public override bool Add(TypeAmount t)
		{
			return !Contains(t) && Set(t.Type, t.Amount, t.State, t.Inherit);
		}

		public override bool Add(Type type)
		{
			return !Contains(type) && SetAmount(type, 1);
		}

		public bool TryGetAmount(Type type, out int amount)
		{
			var success = TryGetEntry(type, out _, out var e);

			if (success)
			{
				amount = e.Amount;
			}
			else
			{
				amount = default;
			}

			return success;
		}

		public virtual bool Set(Type type, int amount, bool state, bool inherit)
		{
			if (type == null)
			{
				return false;
			}

			if (!IsValidType(type))
			{
				return false;
			}

			amount = Math.Clamp(amount, AmountMin, AmountMax);

			var index = IndexOf(type);

			if (index >= 0)
			{
				var e = _Entries[index];

				e.Type = type;
				e.Amount = amount;
				e.State = state;
				e.Inherit = inherit;
			}
			else
			{
				var e = new TypeAmount
				{
					Type = type,
					Amount = amount,
					State = state,
					Inherit = inherit
				};

				_Entries.Add(e);

				OnAdded(e);
			}

			return true;
		}

		public virtual bool SetAmount(Type type, int amount)
		{
			if (type == null)
			{
				return false;
			}

			if (!IsValidType(type))
			{
				return false;
			}

			amount = Math.Clamp(amount, AmountMin, AmountMax);

			var index = IndexOf(type);

			if (index >= 0)
			{
				var e = _Entries[index];

				e.Type = type;
				e.Amount = amount;

				OnUpdated(e);
			}
			else
			{
				var e = new TypeAmount
				{
					Type = type,
					Amount = amount,
					Inherit = type?.IsAbstract ?? false
				};

				_Entries.Add(e);

				OnAdded(e);
			}

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);

			writer.WriteEncodedInt(AmountMin);
			writer.WriteEncodedInt(AmountMax);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();

			AmountMin = reader.ReadEncodedInt();
			AmountMax = reader.ReadEncodedInt();
		}
	}

	[PropertyObject]
	public class TypeEntry : IEquatable<Type>, IEquatable<TypeEntry>
	{
		protected Type _Type = null;

		[CommandProperty(AccessLevel.Counselor, true)]
		public Type Type { get => _Type; set => _Type = value; }

		protected bool _State = true;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool State { get => _State; set => _State = value; }

		protected bool _Inherit = false;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Inherit { get => _Inherit; set => _Inherit = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual bool IsValid => Type != null;

		[CommandProperty(AccessLevel.Counselor)]
		public virtual bool IsActive => IsValid && State;

		public TypeEntry()
		{
		}

		public TypeEntry(Type type)
		{
			_Type = type;
			_Inherit = type?.IsAbstract ?? false;
		}

		public TypeEntry(Type type, bool state)
		{
			_Type = type;
			_State = state;
			_Inherit = type?.IsAbstract ?? false;
		}

		public TypeEntry(Type type, bool state, bool inherit)
		{
			_Type = type;
			_State = state;
			_Inherit = inherit;
		}

		public TypeEntry(GenericReader reader)
		{
			Deserialize(reader);
		}

		public override string ToString()
		{
			return Type?.Name ?? "Null";
		}

		public override int GetHashCode()
		{
			return Type?.GetHashCode() ?? 0;
		}

		public override bool Equals(object o)
		{
			return ReferenceEquals(this, o) || (o is TypeEntry e && Equals(e)) || (o is Type t && Equals(t));
		}

		public virtual bool Equals(TypeEntry t)
		{
			return ReferenceEquals(this, t) || Type == t?.Type;
		}

		public bool Equals(Type t)
		{
			return Type == t;
		}

		public bool Inherited(Type t)
		{
			return Inherit && (Type?.IsAssignableFrom(t) ?? false);
		}

		public bool Match(object o)
		{
			if (o == null)
			{
				return false;
			}

			var t = o.GetType();

			return Equals(t) || Inherited(t);
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.WriteObjectType(_Type);
			writer.Write(_State);
			writer.Write(_Inherit);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.ReadEncodedInt();

			_Type = reader.ReadObjectType();
			_State = reader.ReadBool();
			_Inherit = reader.ReadBool();
		}

		public static bool operator ==(TypeEntry l, TypeEntry r)
		{
			return l?.Type == r?.Type;
		}

		public static bool operator !=(TypeEntry l, TypeEntry r)
		{
			return l?.Type != r?.Type;
		}

		public static bool operator ==(TypeEntry l, Type r)
		{
			return l?.Type == r;
		}

		public static bool operator !=(TypeEntry l, Type r)
		{
			return l?.Type != r;
		}

		public static bool operator ==(Type l, TypeEntry r)
		{
			return l == r?.Type;
		}

		public static bool operator !=(Type l, TypeEntry r)
		{
			return l != r?.Type;
		}

		public static implicit operator Type(TypeEntry t)
		{
			return t?.Type ?? default;
		}

		public static implicit operator bool(TypeEntry t)
		{
			return t?.State ?? default;
		}
	}

	[PropertyObject]
	public class TypeList<T> : ICollection<T> where T : TypeEntry, new()
	{
		protected readonly List<T> _Entries = new();

		bool ICollection<T>.IsReadOnly => ((ICollection<T>)_Entries).IsReadOnly;

		public T this[int index] => _Entries[index];

		public virtual object this[Type type]
		{
			get => TryGetEntry(type, out _, out var t) ? t : null;
			set
			{
				if (value is null)
				{
					Remove(type);
				}
				else if (value is bool s)
				{
					SetState(type, s);
				}
				else if (value is Type t)
				{
					if (type != t)
					{
						Remove(type);
					}

					Add(t);
				}
				else if (value is T e)
				{
					if (type != e.Type)
					{
						Remove(type);
					}

					Add(e);
				}
			}
		}

		public IEnumerable<Type> Types => _Entries.Select(e => e.Type);
		public IEnumerable<bool> States => _Entries.Select(e => e.State);

		public int Count => _Entries.Count;

		public IEnumerable<T> ValidEntries => _Entries.Where(e => e.IsValid);
		public IEnumerable<T> ActiveEntries => _Entries.Where(e => e.IsActive);

		public int ValidCount => _Entries.Count(e => e.IsValid);
		public int ActiveCount => _Entries.Count(e => e.IsActive);

		[CommandProperty(AccessLevel.GameMaster)]
		public string AddTypeByName
		{
			get => String.Empty;
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					return;
				}

				var type = ScriptCompiler.FindTypeByName(value, true);

				if (type != null)
				{
					_ = Add(type);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string RemoveTypeByName
		{
			get => String.Empty;
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					return;
				}

				var type = ScriptCompiler.FindTypeByName(value, true);

				if (type != null)
				{
					_ = Remove(type);
				}
			}
		}

		public TypeList()
		{
			OnInit();
		}

		public TypeList(params T[] entries)
		{
			OnInit();

			_Entries.AddRange(entries);
		}

		public TypeList(IEnumerable<T> entries)
		{
			OnInit();

			_Entries.AddRange(entries);
		}

		public TypeList(GenericReader reader)
		{
			OnInit();

			Deserialize(reader);
		}

		protected virtual void OnInit()
		{
		}

		public virtual bool IsValidType(Type type)
		{
			return type != null;
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		public virtual bool Add(T t)
		{
			return !Contains(t) && Set(t.Type, t.State, t.Inherit);
		}

		public virtual bool Add(Type type)
		{
			return !Contains(type) && SetState(type, true);
		}

		public virtual bool Remove(T t)
		{
			if (_Entries.Remove(t))
			{
				OnRemoved(t);

				return true;
			}

			return false;
		}

		public virtual bool Remove(Type type)
		{
			var index = IndexOf(type);

			if (index >= 0)
			{
				RemoveAt(index);

				return true;
			}

			return false;
		}

		public void RemoveAt(int index)
		{
			if (index >= 0 && index < _Entries.Count)
			{
				var e = _Entries[index];

				_Entries.RemoveAt(index);

				OnRemoved(e);
			}
		}

		public bool Contains(T t)
		{
			return _Entries.Contains(t);
		}

		public bool Contains(Type type)
		{
			return IndexOf(type) >= 0;
		}

		public int IndexOf(T t)
		{
			return _Entries.IndexOf(t);
		}

		public int IndexOf(Type type)
		{
			return IndexOf(type, true);
		}

		public int IndexOf(Type type, bool inherited)
		{
			return _Entries.FindIndex(e => e.IsValid && (e.Equals(type) || (inherited && e.Inherited(type))));
		}

		public bool TryGetEntry(Type type, out int index, out T t)
		{
			index = IndexOf(type);

			if (index >= 0)
			{
				t = _Entries[index];
			}
			else
			{
				t = default;
			}

			return index >= 0;
		}

		public bool TryGetState(Type type, out bool state)
		{
			var success = TryGetEntry(type, out _, out var e);

			if (success)
			{
				state = e.State;
			}
			else
			{
				state = default;
			}

			return success;
		}

		public bool TryGetInherit(Type type, out bool inherit)
		{
			var success = TryGetEntry(type, out _, out var e);

			if (success)
			{
				inherit = e.Inherit;
			}
			else
			{
				inherit = default;
			}

			return success;
		}

		public virtual bool Set(Type type, bool state, bool inherit)
		{
			if (type == null)
			{
				return false;
			}

			if (!IsValidType(type))
			{
				return false;
			}

			var index = IndexOf(type);

			if (index >= 0)
			{
				var e = _Entries[index];

				e.Type = type;
				e.State = state;
				e.Inherit = inherit;

				OnUpdated(e);
			}
			else
			{
				var e = new T
				{
					Type = type,
					State = state,
					Inherit = inherit
				};

				_Entries.Add(e);

				OnAdded(e);
			}

			return true;
		}

		public bool SetState(Type type, bool state)
		{
			if (type == null)
			{
				return false;
			}

			if (!IsValidType(type))
			{
				return false;
			}

			var index = IndexOf(type);

			if (index >= 0)
			{
				var e = _Entries[index];

				e.Type = type;
				e.State = state;

				OnUpdated(e);
			}
			else
			{
				var e = new T
				{
					Type = type,
					State = state,
					Inherit = type?.IsAbstract ?? false
				};

				_Entries.Add(e);

				OnAdded(e);
			}

			return true;
		}

		public void SetInherit(Type type, bool inherit)
		{
			if (type == null)
			{
				return;
			}

			if (!IsValidType(type))
			{
				return;
			}

			var index = IndexOf(type);

			if (index >= 0)
			{
				var e = _Entries[index];

				e.Type = type;
				e.Inherit = inherit;

				OnUpdated(e);
			}
			else
			{
				var e = new T
				{
					Type = type,
					Inherit = inherit
				};

				_Entries.Add(e);

				OnAdded(e);
			}
		}

		protected virtual void OnAdded(T t)
		{
		}

		protected virtual void OnRemoved(T t)
		{
		}

		protected virtual void OnUpdated(T t)
		{
		}

		public void Clear()
		{
			_Entries.Clear();
		}

		public void CopyTo(TypeList<T> obj)
		{
			foreach (var o in _Entries)
			{
				obj.Add(o);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_Entries.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _Entries.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Entries.GetEnumerator();
		}

		public override string ToString()
		{
			return $"Types[{Count}]";
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.WriteEncodedInt(_Entries.Count);

			foreach (var e in _Entries)
			{
				e.Serialize(writer);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.ReadEncodedInt();

			var count = reader.ReadEncodedInt();

			while (--count >= 0)
			{
				var e = new T();

				e.Deserialize(reader);

				if (e.Type != null && IsValidType(e.Type))
				{
					_Entries.Add(e);
				}
			}
		}
	}
}
