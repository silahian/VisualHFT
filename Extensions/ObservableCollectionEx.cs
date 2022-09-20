using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.Data.PropertyGrid;

namespace VisualHFT.Extensions
{
    /// <summary>
    /// Expanded ObservableCollection to include some List<T> Methods
    /// </summary>
    [Serializable]
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private Comparison<T> _comparison; //= new Comparison<T>((bd1, bd2) => { return DateTime.Compare(bd1.StartDate, bd2.StartDate); });

        public Comparison<T> Comparison { get => _comparison; set => _comparison = value; }

        /// <summary>
        /// Constructors
        /// </summary>
        public ObservableCollectionEx(Comparison<T> comparison) : base()
        {
            _comparison = comparison;
        }
        public ObservableCollectionEx(List<T> l, Comparison<T> comparison) : base(l)
        {
            _comparison = comparison;
        }
        public ObservableCollectionEx(IEnumerable<T> l) : base(l) 
        {
            
        }

        public new void Add(T item)
        {
            InternalInsert(item);
        }

        private void InternalInsert(T item)
        {            
            try
            {
                
                if (base.Count == 0)
                    base.Add(item);
                else
                {
                    bool last = true;
                    for(int i=0; i < base.Count; i++)
                    {
                        int result = _comparison.Invoke(base[i], item);
                        if (result >= 1)
                        {
                            base.Insert(i, item);
                            last = false;
                            break;
                        }
                    }
                    if (last)
                    {
                        base.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Sort()
        {
            var sortableList = this.ToList();
            sortableList.Sort(_comparison);
            bool isSortDone = false;

            while (!isSortDone)
            {
                isSortDone = true;
                for (int i = 0; i < sortableList.Count; i++)
                {
                    if (base.IndexOf(sortableList[i]) != i)
                    {
                        var replacedItem = base[i];
                        base.Move(base.IndexOf(sortableList[i]), i);
                        if (base.IndexOf(sortableList[i]) < i-1)
                        {
                            isSortDone=false;
                            break;
                        }
                    }
                }
            }

        }

    }

}
