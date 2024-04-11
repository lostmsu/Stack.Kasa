namespace Stack.Kasa;

using System.Windows.Input;

using Core = global::Kasa;

using LostTech.Stack.Widgets.DataBinding;
using LostTech.Stack.Widgets.DataSources;

using Prism.Commands;
using System.IO;
using System.Net.Sockets;
using global::Kasa;

public sealed class KasaOutlet: DependencyObjectNotifyBase, IRefreshable {
    IKasaOutlet.IEnergyMeterCommands? outlet;
    string? hostOrIP = null;
    string? childID = null;
    public string? Error { get; private set; }

    public KasaOutlet() {
        this.RefreshCommand = new DelegateCommand(this.TryRefreshInternal);
    }

    public string? HostOrIP {
        get => this.hostOrIP;
        set {
            this.hostOrIP = value;
            this.UpdateOutlet();
        }
    }

    public string? ChildID {
        get => this.childID;
        set {
            this.childID = value;
            this.UpdateOutlet();
        }
    }

    void UpdateOutlet() {
        this.outlet =
            this.hostOrIP is null
            ? null :
                (this.childID is null
                ? new Core.KasaOutlet(this.hostOrIP).EnergyMeter
                : new Core.KasaStripOutlet(this.hostOrIP, this.childID));
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
            this.UpdateOutlet();
        }
        try {
            power = await this.outlet.GetInstantaneousPowerUsage();
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
