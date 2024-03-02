using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyCSharp;

public abstract class NotifyPropertyClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
}
