using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

class ClockForm : Form
{
    //stopwatch parts
    Label LabelS;
    TextBox Output;
    Button MyStartButton;
    Button MyCancelButton;
    Button MyPauseButton;
    Button MyPlayButton;
    Thread StopwatchThread;

    //timer parts
    Label LabelT;
    TextBox Input;
    TextBox OutputT;
    Button MyStartButtonT;
    Button MyCancelButtonT;
    Button MyPauseButtonT;
    Button MyPlayButtonT;
    Thread TimerThread;
    int totalSecs;

    public ClockForm()
    {
        // Initialize the form's properties
        Text = "Clocks";
        ClientSize = new System.Drawing.Size(660, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        // Instantiate stopwatch controls
        LabelS = new Label();
        Output = new TextBox();
        MyStartButton = new Button();
        MyCancelButton = new Button();
        MyPauseButton = new Button();
        MyPlayButton = new Button();

        // Instantiate timer controls
        LabelT = new Label();
        Input = new TextBox();
        OutputT = new TextBox();
        MyStartButtonT = new Button();
        MyCancelButtonT = new Button();
        MyPauseButtonT = new Button();
        MyPlayButtonT = new Button();

        // Initialize the controls
        LabelS.Location = new Point(120, 28);
        LabelS.Size = new Size(250, 16);
        LabelS.Text = "Stopwatch";

        Output.Location = new Point(24, 64);
        Output.Size = new Size(240, 20);
        Output.Name = "Output";
        Output.ReadOnly = true;
        Output.TabStop = false;

        MyStartButton.Location = new Point(24, 104);
        MyStartButton.Size = new Size(104, 32);
        MyStartButton.Text = "Start";
        MyStartButton.TabIndex = 1;
        MyStartButton.Click += new EventHandler(OnStartStopwatch);

        MyCancelButton.Location = new Point(160, 104);
        MyCancelButton.Size = new Size(104, 32);
        MyCancelButton.Text = "Cancel";
        MyCancelButton.TabIndex = 2;
        MyCancelButton.Enabled = false;
        MyCancelButton.Click += new EventHandler(OnCancelStopwatch);

        MyPauseButton.Location = new Point(24, 154);
        MyPauseButton.Size = new Size(104, 32);
        MyPauseButton.Text = "Pause";
        MyPauseButton.TabIndex = 1;
        MyPauseButton.Enabled = false;
        MyPauseButton.Click += new EventHandler(OnPauseStopWatch);

        MyPlayButton.Location = new Point(160, 154);
        MyPlayButton.Size = new Size(104, 32);
        MyPlayButton.Text = "Play";
        MyPlayButton.TabIndex = 2;
        MyPlayButton.Enabled = false;
        MyPlayButton.Click += new EventHandler(OnPlayStopWatch);

        LabelT.Location = new Point(384, 28);
        LabelT.Size = new Size(144, 16);
        LabelT.Text = "Timer";

        Input.Location = new Point(428, 26);
        Input.Size = new Size(196, 20);
        Input.Name = "Input";
        Input.Enabled = true;
        Input.ReadOnly = false;
        Input.TabIndex = 0;

        OutputT.Location = new Point(384, 64);
        OutputT.Size = new Size(240, 20);
        OutputT.Name = "Output";
        OutputT.ReadOnly = true;
        OutputT.TabStop = false;

        MyStartButtonT.Location = new Point(384, 104);
        MyStartButtonT.Size = new Size(104, 32);
        MyStartButtonT.Text = "Start";
        MyStartButtonT.TabIndex = 1;
        MyStartButtonT.Click += new EventHandler(OnStartTimer);

        MyCancelButtonT.Location = new Point(520, 104);
        MyCancelButtonT.Size = new Size(104, 32);
        MyCancelButtonT.Text = "Cancel";
        MyCancelButtonT.TabIndex = 2;
        MyCancelButtonT.Enabled = false;
        MyCancelButtonT.Click += new EventHandler(OnCancelTimer);

        MyPauseButtonT.Location = new Point(384, 154);
        MyPauseButtonT.Size = new Size(104, 32);
        MyPauseButtonT.Text = "Pause";
        MyPauseButtonT.TabIndex = 1;
        MyPauseButtonT.Enabled = false;
        MyPauseButtonT.Click += new EventHandler(OnPauseTimer);

        MyPlayButtonT.Location = new Point(520, 154);
        MyPlayButtonT.Size = new Size(104, 32);
        MyPlayButtonT.Text = "Play";
        MyPlayButtonT.TabIndex = 2;
        MyPlayButtonT.Enabled = false;
        MyPlayButtonT.Click += new EventHandler(OnPlayTimer);

        // Add the controls to the form
        Controls.Add(LabelS);
        Controls.Add(Output);
        Controls.Add(MyStartButton);
        Controls.Add(MyCancelButton);
        Controls.Add(MyPauseButton);
        Controls.Add(MyPlayButton);

        Controls.Add(LabelT);
        Controls.Add(Input);
        Controls.Add(OutputT);
        Controls.Add(MyStartButtonT);
        Controls.Add(MyCancelButtonT);
        Controls.Add(MyPauseButtonT);
        Controls.Add(MyPlayButtonT);

        Input.BringToFront();
        this.Shown += (s, e) => { this.ActiveControl = MyStartButton; }; //allows exmaple input to be shown
        Input.Text = "Enter time (SS, MM:SS, or HH:MM:SS)";
        Input.ForeColor = Color.Gray;
        Input.GotFocus += (sender, e) =>
        {
            if (Input.ForeColor == Color.Gray)
            {
                Input.Text = "";
                Input.ForeColor = Color.Black;
            }
        };

        Input.LostFocus += (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(Input.Text))
            {
                Input.Text = "Enter time (SS, MM:SS, or HH:MM:SS)";
                Input.ForeColor = Color.Gray;
            }
        };
    }

    volatile bool IsWatchRunning = false;
    volatile bool IsTimerRunning = false;
    volatile bool IsWatchPaused = false;
    volatile bool IsTimerPaused = false;
    ManualResetEventSlim watchGate = new ManualResetEventSlim(true); //ensures the clocks pauses immediately 
    ManualResetEventSlim timerGate = new ManualResetEventSlim(true);

    void UI(Action a)
    {
        if (InvokeRequired) BeginInvoke(a);
        else a();
    }
    void OnStartStopwatch(object sender, EventArgs e)
    {
        if (IsWatchRunning) return;
        IsWatchRunning = true;

        UI(() =>
        {
            MyStartButton.Enabled = false;
            MyCancelButton.Enabled = true;
            MyPauseButton.Enabled = true;
            StopwatchThread = null;
        });

        // Start a background thread for the stopwatch
        watchGate.Set();
        StopwatchThread = new Thread(new ThreadStart(StopwatchFunc));
        StopwatchThread.IsBackground = true;
        StopwatchThread.Start();
    }
    void OnCancelStopwatch(object sender, EventArgs e)
    {
        IsWatchRunning = false;
        IsWatchPaused = false;
        watchGate.Set();
        MyStartButton.Enabled = true;
        MyPauseButton.Enabled = false;
        MyPlayButton.Enabled = false;
        MyCancelButton.Enabled = false;
    }
    void OnPauseStopWatch(object sender, EventArgs e)
    {
        IsWatchPaused = true;
        watchGate.Reset();
        MyPauseButton.Enabled = false;
        MyPlayButton.Enabled = true;
    }
    void OnPlayStopWatch(object sender, EventArgs e)
    {
        IsWatchPaused = false;
        watchGate.Set();
        MyPauseButton.Enabled = true;
        MyPlayButton.Enabled = false;
    }
    void StopwatchFunc()
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        bool wasPaused = false;

        while (IsWatchRunning)
        {
            if (IsWatchPaused)
            {
                if (!wasPaused)
                {
                    watch.Stop();
                    wasPaused = true;
                }
                watchGate.Wait();
                continue;
            }
            else if (wasPaused)
            {
                watch.Start();
                wasPaused = false;
            }
            UI(() => Output.Text = $"{(int)watch.Elapsed.TotalHours:00}:{watch.Elapsed.Minutes:00}:{watch.Elapsed.Seconds:00}");
            Thread.Sleep(100);
        }
        watch.Stop();
        UI(() =>
        {
            MyStartButton.Enabled = true;
            MyCancelButton.Enabled = false;
            StopwatchThread = null;
        });
    }

    void OnStartTimer(object sender, EventArgs e)
    {
        if (IsTimerRunning) return;

        string input = Input.Text.Trim();

        if (string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Enter time as SS, MM:SS, or HH:MM:SS.");
            return;
        }

        var parts = input.Split(':');

        if (parts.Length == 1)
        {
            if (!int.TryParse(parts[0], out int secs) || secs < 0 || secs >= 60)
            {
                MessageBox.Show("For SS, seconds must be 0–59.");
                return;
            }
            totalSecs = secs;
        }
        else if (parts.Length == 2)
        {
            if (!int.TryParse(parts[0], out int mins) || !int.TryParse(parts[1], out int secs) ||
            mins < 0 || mins >= 60 || secs < 0 || secs >= 60)
            {
                MessageBox.Show("For MM:SS, minutes ≥ 0 and seconds 0–59.");
                return;
            }
            totalSecs = (mins * 60) + secs;
        }
        else if (parts.Length == 3)
        {
            if (!int.TryParse(parts[0], out int hrs) || !int.TryParse(parts[1], out int mins) ||
            !int.TryParse(parts[2], out int secs) ||
             hrs < 0 || mins < 0 || mins >= 60 || secs < 0 || secs >= 60)
            {
                MessageBox.Show("For HH:MM:SS, hours ≥ 0, minutes 0–59, seconds 0–59.");
                return;
            }
            totalSecs = (hrs * 3600) + (mins * 60) + secs;
        }
        else
        {
            MessageBox.Show("Enter time as SS, MM:SS, or HH:MM:SS.");
            return;
        }

        if (totalSecs < 30)
        {
            MessageBox.Show("Please enter a time of at least 00:00:30.");
            return;
        }

        IsTimerRunning = true;

        UI(() =>
        {
            MyStartButtonT.Enabled = false;
            MyCancelButtonT.Enabled = true;
            MyPauseButtonT.Enabled = true;
            Input.Enabled = false;

            int h = totalSecs / 3600;
            int m = (totalSecs % 3600) / 60;
            int s = totalSecs % 60;
            OutputT.Text = $"{h:00}:{m:00}:{s:00}";
        });

        // Start a background thread for the timer
        timerGate.Set();
        TimerThread = new Thread(new ThreadStart(TimerFunc));
        TimerThread.IsBackground = true;
        TimerThread.Start();
    }

    void OnCancelTimer(object sender, EventArgs e)
    {
        IsTimerRunning = false;
        IsTimerPaused = false;
        timerGate.Set();
        MyStartButtonT.Enabled = true;
        MyPauseButtonT.Enabled= false;
        MyPlayButtonT.Enabled= false;
        MyCancelButtonT.Enabled= false;
    }

    void OnPauseTimer(object sender, EventArgs e)
    {
        IsTimerPaused = true;
        timerGate.Reset();  
        MyPauseButtonT.Enabled = false;
        MyPlayButtonT.Enabled = true;
    }

    void OnPlayTimer (object sender, EventArgs e)
    {
        IsTimerPaused = false;
        timerGate.Set();
        MyPauseButtonT.Enabled = true;
        MyPlayButtonT.Enabled = false;
    }

    void TimerFunc()
    {
        int timeLeft = totalSecs;
        bool wasPaused = false;

        while (IsTimerRunning && timeLeft > 0)
        {
            if (IsTimerPaused)
            {
                if (!wasPaused)
                {
                    wasPaused = true;
                }
                timerGate.Wait();
                continue;
            }
            else if (wasPaused)
            {
                wasPaused= false;
            }

            Thread.Sleep(1000);
            // if either happens during sleep make sure the number doesnt decrement
            if (IsTimerPaused) continue;
            if (!IsTimerRunning) break;
            timeLeft -= 1;

            int h = timeLeft / 3600;
            int m = (timeLeft % 3600) / 60;
            int s = timeLeft % 60;

            UI(() => OutputT.Text = $"{h:00}:{m:00}:{s:00}");
        }

        IsTimerRunning = false;

        UI(() =>
        {
            MyStartButtonT.Enabled = true;
            MyCancelButtonT.Enabled = false;
            MyPauseButtonT.Enabled = false;
            Input.Enabled = true;
            TimerThread = null;
        });
    }
    static void Main()
    {
        Application.Run(new ClockForm());
    }
}
