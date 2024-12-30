using System.ComponentModel;
using Avalonia.Media;

namespace QArantine.Code.QArantineGUI.Models
{
    public class GUILogLine : INotifyPropertyChanged
    {
        private string _timestamp;
        private IBrush _timestampForeground;
        private string _testTag;
        private IBrush _testTagForeground;
        private string _logBody;
        private IBrush _logBodyForeground;

        public string Timestamp
        {
            get { return _timestamp; }
            set  { _timestamp = value; }
        }

        public IBrush TimestampForeground
        {
            get { return _timestampForeground; }
            set { _timestampForeground = value; }
        }

        public string TestTag
        {
            get { return _testTag; }
            set { _testTag = value; }
        }

        public IBrush TestTagForeground
        {
            get { return _testTagForeground; }
            set {  _testTagForeground = value; }
        }

        public string LogBody
        {
            get { return _logBody; }
            set { _logBody = value; }
        }

        public IBrush LogBodyForeground
        {
            get { return _logBodyForeground; }
            set { _logBodyForeground = value; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public GUILogLine(string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            _timestamp = timestamp ?? "!NullTextFound!";
            _timestampForeground = timestampForeground ?? Brushes.Blue;
            _testTag = testTag ?? "!NullTextFound!";
            _testTagForeground = testTagForeground ?? Brushes.Blue;
            _logBody = logBody ?? "!NullTextFound!";
            _logBodyForeground = logBodyForeground ?? Brushes.Blue;
            RaisePropertyChanged("");
        }

        public GUILogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            _timestamp = timestamp ?? "!NullTextFound!";
            _timestampForeground = !string.IsNullOrEmpty(timestampForegroundHexCode) ? new SolidColorBrush(Color.Parse(timestampForegroundHexCode)) : Brushes.Blue;
            _testTag = testTag ?? "!NullTextFound!";
            _testTagForeground = !string.IsNullOrEmpty(testTagForegroundHexCode) ? new SolidColorBrush(Color.Parse(testTagForegroundHexCode)) : Brushes.Blue;
            _logBody = logBody ?? "!NullTextFound!";
            _logBodyForeground = !string.IsNullOrEmpty(logBodyForegroundHexCode) ? new SolidColorBrush(Color.Parse(logBodyForegroundHexCode)) : Brushes.Blue;
            RaisePropertyChanged("");
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return _timestamp + " " + _testTag + " " + _logBody;
        }
    }
}