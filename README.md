Stack Widgets for Kasa smart devices

## Example usage

1. Add the following definitions to your layout:
```xml
xmlns:dataSources="clr-namespace:LostTech.Stack.Widgets.DataSources;assembly=LostTech.Stack.Widgets"
xmlns:kasa="clr-namespace:Stack.Kasa;assembly=Stack.Kasa"
```

2. Create a sensor
```xml
<Grid.Resources>
  <kasa:KasaOutlet x:Key="PowerMeter"
                   HostOrIP="192.168.1.2"
                   dataSources:DataSource.RefreshInterval="0:0:5" />
</Grid.Resources>
```

3. Bind a `TextBlock`
```xml
<TextBlock Text="{Binding Power, Source={StaticResource PowerMeter}, Mode=OneWay, StringFormat={}{0:0}W}"
           Background="White" Padding="7,3" />
```

4. Build the project and copy the following files to the `CustomWidgets` folder
(it should be next to your `Layouts` folder):
`Stack.Kasa.dll`, `Newtonsoft.Json.dll`, `slf4net.dll`, and `Kasa.dll`.

5. Restart Stack and load the updated layout.