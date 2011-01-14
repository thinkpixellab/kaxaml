namespace Kaxaml.Plugins.XamlScrubber.Properties {
    
    internal sealed partial class Settings {

        public Settings()
        {
            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Settings_PropertyChanged);
        }

        void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Settings.Default.Save();
        }
    }
}
