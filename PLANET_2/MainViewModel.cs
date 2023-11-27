using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace PLANET_2
{
	internal class MainViewModel : ObservableObject
	{
		public ObservableCollection<MyProcess> mprocessListObservable { get; set; }
		public ObservableCollection<string> processListObservable { get; set; }
		public int refreshTime { get; set; }
		public string priority { get; set; }
		public string filterText { get; set; }
		public int selectedIndex
		{
			get => _selectedIndex;
			set
			{
				if (_selectedIndex != null)
				{
					_selectedIndex = value;
					SetProcessInfo();
				}
			}
		}
		private int _selectedIndex;

		public List<ProcessPriorityClass> priorityClasses { get; }
		public int selectedPriority
		{
			get => _selectedPriority;
			set
			{
				if (_selectedPriority != value)
				{
					_selectedPriority = value;
					_priority = priorityClasses.ElementAt(value);
				}
				Trace.WriteLine(_priority.ToString());
			}
		}
		private int _selectedPriority;
		private ProcessPriorityClass _priority;
		public string processName
		{
			get => _processName;
			set
			{
				_processName = value;
				OnPropertyChanged();
			}
		}
		private string _processName = "no data";
		public int processID
		{
			get => _processID;
			set
			{
				_processID = value;
				OnPropertyChanged();
			}
		}
		private int _processID = 0;
		public string processPriority
		{
			get => _processPriority;
			set
			{
				if (value == null)
					_processPriority = "No Permission";
				else
					_processPriority = value;
				OnPropertyChanged();
			}
		}
		private string _processPriority = "no data";
		public string processTime
		{
			get => _processTime;
			set
			{
				if (value == null)
					_processTime = "No Permission";
				else
					_processTime = value;
				OnPropertyChanged();
			}
		}
		private string _processTime;
		public string processMemory
		{
			get => _processMemory;
			set
			{
				if (value == null)
					_processMemory = "No Permission";
				else
					_processMemory = value;
				OnPropertyChanged();
			}
		}
		private string _processMemory;


		public ICommand sortCommand { get; }
		public ICommand refreshCommand { get; }
		public ICommand startCommand { get; }
		public ICommand stopCommand { get; }
		public ICommand filterCommand { get; }
		public ICommand setPriorityCommand { get; }
		public ICommand killCommand { get; }

		private DispatcherTimer _timer = new DispatcherTimer();


		public MainViewModel()
		{
			priorityClasses = Enum.GetValues(typeof(ProcessPriorityClass)).Cast<ProcessPriorityClass>().ToList();
			mprocessListObservable = new ObservableCollection<MyProcess>(
				Process.GetProcesses().Select(
					item => new MyProcess(item)
					/*{
						name = item.ProcessName.ToString(),
						id = item.Id,
						priority = item.PriorityClass.ToString() != null ? item.PriorityClass.ToString() : "",
					}*/
					).ToList());
			processListObservable = new ObservableCollection<string>(
				Process.GetProcesses().Select(item => item.ProcessName.ToString()).ToList()
				);

			sortCommand = new RelayCommand(Sort);
			refreshCommand = new RelayCommand(Refresh);
			startCommand = new RelayCommand(Start);
			stopCommand = new RelayCommand(Stop);
			filterCommand = new RelayCommand(Filter);
			setPriorityCommand = new RelayCommand(SetPriority);
			killCommand = new RelayCommand(KillProcess);

			_timer.Tick += TimerTick;
		}

		private void TimerTick(object sender, EventArgs e)
		{
			Refresh(null);
		}
		private void Refresh(object obj)
		{
			Trace.WriteLine("Refresh");

			processListObservable.Clear();
			foreach (var process in Process.GetProcesses().Select(item => item.ProcessName.ToString()).ToList())
			{
				processListObservable.Add(process);
			}
		}
		private void Sort(object obj)
		{
			Trace.WriteLine("Sort");
			var sortedCollection = new ObservableCollection<string>(processListObservable.OrderBy(x => x));

			processListObservable.Clear();
			foreach (var item in sortedCollection)
			{
				processListObservable.Add(item);
			}
		}
		private void Start(object obj)
		{
			Trace.WriteLine("Start with refresh time:" + refreshTime.ToString());

			_timer.Interval = TimeSpan.FromMilliseconds(refreshTime);
			_timer.Start();
		}
		private void Stop(object obj)
		{
			Trace.WriteLine("Stop");

			_timer.Stop();
		}
		private void Filter(object obj)
		{
			Trace.WriteLine("Filter");
			var collectionCopy = new ObservableCollection<string>(processListObservable);

			processListObservable.Clear();
			foreach (var item in collectionCopy)
			{
				if (item.Contains(filterText))
				{
					processListObservable.Add(item);
				}
			}
		}
		private void SetPriority(object obj)
		{
			Trace.WriteLine("Change priority [" + selectedIndex.ToString() + "] to: " + "");

			var procName = processListObservable.ElementAt(selectedIndex);
			var proc = Process.GetProcesses().First(item => item.ProcessName == procName);

			try
			{
				proc.PriorityClass = _priority;
			}
			catch (Exception)
			{
				Trace.WriteLine("Error");
			}
			SetProcessInfo();
		}
		private void KillProcess(object obj)
		{
			Trace.WriteLine("Kill process [" + selectedIndex.ToString() + "]");

			var procName = processListObservable.ElementAt(selectedIndex);
			var proc = Process.GetProcesses().First(item => item.ProcessName == procName);

			try
			{
				proc.Kill();
			}
			catch (Exception)
			{
				Trace.WriteLine("Error");
			}
			Refresh(null);
		}
		private void SetProcessInfo()
		{
			Trace.WriteLine("Process info [" + selectedIndex.ToString() + "]");

			var procName = processListObservable.ElementAt(selectedIndex);
			var proc = Process.GetProcesses().First(item => item.ProcessName == procName);

			processName = proc.ProcessName;
			processID = proc.Id;

			try
			{
				processMemory = proc.VirtualMemorySize.ToString();
				processTime = proc.TotalProcessorTime.TotalSeconds.ToString();
				processPriority = proc.PriorityClass.ToString();
			}
			catch (Exception ex)
			{
				processTime = "No Permission";
				processPriority = "No Permission";
			}
		}
	}
	public class MyProcess
	{
		public MyProcess(Process proc)
		{
			this.name = proc.ProcessName.ToString();
			this.id = proc.Id;
			try
			{
				this.priority = proc.PriorityClass.ToString();
			}
			catch
			{
				this.priority = "no access";
			}
		}
		public string name { get; set; }
		public int id { get; set; }
		public string priority { get; set; }
	}

	public class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public class RelayCommand : ICommand
	{
		private Action<object> execute;
		private Func<object, bool> canExecute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return this.canExecute == null || this.canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			this.execute(parameter);
		}
	}
}