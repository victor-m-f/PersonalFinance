using System.Collections.ObjectModel;

namespace PersonalFinance.Ui.Shared.Collections;
public sealed class ObservableCollectionEx<T> : ObservableCollection<T>
{
    public void ReplaceRange(IEnumerable<T> items)
    {
        Items.Clear();

        foreach (var item in items)
        {
            Items.Add(item);
        }

        OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
            System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
    }
}