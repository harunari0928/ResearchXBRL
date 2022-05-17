using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Renci.SshNet;
using ResearchXBRL.Infrastructure.Shared.FileStorages;

namespace ResearchXBRL.Infrastructure.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// IFileStorageにSFTPFileStorageをDIする
    /// 以下環境変数を設定する必要がある
    /// FILESTORAGE_HOST: SFTPサーバのホスト名(必須)
    /// FILESTORAGE_USERID: SFTPサーバのユーザID(必須)
    /// FILESTORAGE_PASSWORD: SFTPサーバのパスワード(必須)
    /// FILESTORAGE_BASEDIR: SFTPサーバで使用するディレクトリ(任意)
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddSFTPFileStorage(this IServiceCollection serviceCollection)
    {
        var host = Environment.GetEnvironmentVariable("FILESTORAGE_HOST");
        var user = Environment.GetEnvironmentVariable("FILESTORAGE_USERID");
        var password = Environment.GetEnvironmentVariable("FILESTORAGE_PASSWORD");
        var connectionInfo = new ConnectionInfo(host, user,
            new PasswordAuthenticationMethod(user, password));
        var client = new SftpClient(connectionInfo);
        client.Connect();
        var baseDirectory = Environment.GetEnvironmentVariable("FILESTORAGE_BASEDIR") ?? "~/";
        return serviceCollection
            .AddTransient<ISftpClient>(_ => client)
            .AddTransient<IFileStorage>(x => new SFTPFileStorage(x.GetService<ISftpClient>()!, baseDirectory));
    }

    /// <summary>
    /// ログモジュールをDIする
    /// ログ出力にはNLogを用いる
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            logging.AddNLog("nlog.config.xml");
            logging.AddFilter("Microsoft", LogLevel.Warning);
            logging.AddFilter("System", LogLevel.Warning);
            logging.AddFilter("NToastNotify", LogLevel.Warning);
        });
    }
}
