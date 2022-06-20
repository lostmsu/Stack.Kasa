namespace Stack.Kasa;

using System.Windows.Input;

using Core = global::Kasa;

using LostTech.Stack.Widgets.DataBinding;
using LostTech.Stack.Widgets.DataSources;

using Prism.Commands;
using System.IO;
using System.Net.Sockets;

public sealed class KasaOutlet : DependencyObjectNotifyBase, IRefreshable {
    Core.IKasaOutlet? outlet;
    public string? Error { get; private set; }

    public KasaOutlet() {
        this.RefreshCommand = new DelegateCommand(this.TryRefreshInternal);
    }

    public string? HostOrIP {
        get => this.outlet?.Hostname;
        set => this.outlet = value is null ? null : new Core.KasaOutlet(value);
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
        if (this.outlet is null) {
            this.Error = "No outlet selected";
            this.OnPropertyChanged(nameof(this.Error));
            return;
        }
        Core.PowerUsage power;
        void SetErrorAndReconnect(Exception error) {
            this.Error = $"{error.GetType().Name}: {error.Message}";
            this.OnPropertyChanged(nameof(this.Error));
            this.outlet = new Core.KasaOutlet(this.outlet!.Hostname);
        }
        try {
            power = await this.outlet.EnergyMeter.GetInstantaneousPowerUsage();
        } catch (IOException e) {
            SetErrorAndReconnect(e);
            return;
        } catch (SocketException e) {
            SetErrorAndReconnect(e);
            return;
        } catch (InvalidOperationException e) {
            SetErrorAndReconnect(e);
            return;
        } catch (global::Kasa.NetworkException e) {
            SetErrorAndReconnect(e);
            return;
        }
        this.ClearError();
        this.Power = power.Power * 0.001m;
        this.OnPropertyChanged(nameof(this.Power));
        this.Voltage = power.Voltage * 0.001m;
        this.OnPropertyChanged(nameof(this.Voltage));
    }

    void ClearError() {
        if (this.Error is null) return;
        this.Error = null;
        this.OnPropertyChanged(nameof(this.Error));
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
