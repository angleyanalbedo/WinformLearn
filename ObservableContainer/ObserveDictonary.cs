using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableContainer
{
    // 定义一个委托类型，用于处理字典变化的事件
    public delegate void DictionaryChangedEventHandler<TKey, TValue>(object sender, DictionaryChangedEventArgs<TKey, TValue> e);
    // 定义一个枚举类型，用于表示字典变化的类型
    public enum DictionaryChangedType
    {
        Init,
        Add,
        Remove,
        Update,
        Clear,
    }
    // 定义一个事件参数类，用于封装字典变化的信息
    public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {
        // 变化的类型，可以是Add, Remove, Update或Clear
        public DictionaryChangedType ChangeType { get; set; }

        // 变化的键
        public TKey Key { get; set; }

        // 变化的值
        public TValue Value { get; set; }

        // 构造函数
        public DictionaryChangedEventArgs(DictionaryChangedType changeType, TKey key, TValue value)
        {
            ChangeType = changeType;
            Key = key;
            Value = value;
        }
    }
    // 定义一个继承自Dictionary<TKey,TValue>的类，用于触发字典变化的事件    
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
        // 定义一个构造函数，用于接受一个IDictionary<TKey, TValue>类型的参数    
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            DictionaryChanged = delegate { }; // 初始化事件为一个空委托以避免 null  
            base.Clear();
            foreach (var item in dictionary)
            {
                base.Add(item.Key, item.Value);
            }
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Init, default(TKey)!, default(TValue)!));
        }

        public ObservableDictionary()
        {
            DictionaryChanged = delegate { }; // 初始化事件为一个空委托以避免 null  
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Init, default(TKey)!, default(TValue)!));
        }

        // 定义一个事件，用于通知字典变化    
        public event DictionaryChangedEventHandler<TKey, TValue> DictionaryChanged;

        // 重写Add方法，用于在添加元素时触发事件    
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Add, key, value));
        }

        // Fix for CS8600: 将 null 文本或可能的 null 值转换为不可为 null 类型  
        public new bool Remove(TKey key)
        {
            if (base.TryGetValue(key, out TValue? value)) // Use nullable TValue to handle potential null values  
            {
                base.Remove(key);
                OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Remove, key, value!)); // Use null-forgiving operator (!) to ensure non-null value  
                return true;
            }
            return false;
        }

        // 重写索引器，用于在更新元素时触发事件    
        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                    OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Update, key, value));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        // 重写Clear方法，用于在清空字典时触发事件    
        public new void Clear()
        {
            base.Clear();
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(DictionaryChangedType.Clear, default(TKey)!, default(TValue)!));
        }

        // 定义一个虚方法，用于触发事件    
        protected virtual void OnDictionaryChanged(DictionaryChangedEventArgs<TKey, TValue> e)
        {
            DictionaryChanged?.Invoke(this, e);
        }

        public void ResetSubscriptions()
        {
            DictionaryChanged = null;
        }
    }
}
