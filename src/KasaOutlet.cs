namespace Stack.Kasa;

using System.Windows.Input;

using Core = global::Kasa;

using LostTech.Stack.Widgets.DataBinding;
using LostTech.Stack.Widgets.DataSources;

using Prism.Commands;

public sealed class KasaOutlet : DependencyObjectNotifyBase, IRefreshable {
    readonly Core.IKasaOutlet outlet;
    public string? Error { get; private set; }

    public KasaOutlet(string hostOrIP) {
        this.outlet = new Core.KasaOutlet(hostOrIP);
        this.RefreshCommand = new DelegateCommand(this.TryRefreshInternal);
    }

    /// <summary>
    /// Currently consumed power (in watts)
    /// </summary>
    public decimal Power { get; private set; }
    /// <summary>
    /// Current voltage (volts)
    /// </summary>
    public decimal Voltage { get; private set; }

    public ICommand RefreshCommand { get; }

    async void RefreshInternal() {
        var power = await this.outlet.EnergyMeter.GetInstantaneousPowerUsage();
        this.Power = power.Power * 0.001m;
        this.OnPropertyChanged(nameof(this.Power));
        this.Voltage = power.Voltage * 0.001m;
        this.OnPropertyChanged(nameof(this.Voltage));
    }

    void TryRefreshInternal() {
        bool hadError = this.Error is not null;
        try {
            this.RefreshInternal();
        } catch (InvalidOperationException) {
            this.OnPropertyChanged(nameof(this.Error));
            return;
        }
        if (hadError)
            this.OnPropertyChanged(nameof(this.Error));
    }
}
