// Subs.Ext\Tools\Rating.WPF\Services\ISettingsService.cs

using Rating.WPF.Models;

namespace Rating.WPF.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
    }
}
