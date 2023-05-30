using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChatPresetTool
{
    public class ClipboardBackup : IDisposable
    {
        private readonly Stream _audioBackup;
        private readonly StringCollection _fileDropBackup;
        private readonly BitmapSource _imageBackup;
        private readonly string _textBackup;

        public ClipboardBackup()
        {
            _audioBackup = Clipboard.ContainsAudio() ? Clipboard.GetAudioStream() : null;
            _fileDropBackup = Clipboard.ContainsFileDropList() ? Clipboard.GetFileDropList() : null;
            _imageBackup = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            _textBackup = Clipboard.ContainsText() ? Clipboard.GetText() : "";
        }

        public void Dispose()
        {
            if (_audioBackup != null)
            {
                Clipboard.SetAudio(_audioBackup);
            }

            if (_fileDropBackup != null)
            {
                Clipboard.SetFileDropList(_fileDropBackup);
            }

            if (_imageBackup != null)
            {
                Clipboard.SetImage(_imageBackup);
            }

            if (_textBackup != "")
            {
                Clipboard.SetText(_textBackup);
            }
        }
    }
}