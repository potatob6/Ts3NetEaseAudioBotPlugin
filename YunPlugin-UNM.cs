﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nini.Config;
using TS3AudioBot;
using TS3AudioBot.Audio;
using TS3AudioBot.CommandSystem;
using TS3AudioBot.Plugins;
using TSLib.Full;
using NeteaseCloudMusicApi;
using System.Web;

public class YunPlugin : IBotPlugin 
{
    //===========================================初始化===========================================
    static IConfigSource MyIni;
    PlayManager tempplayManager;
    InvokerData tempinvoker;
    Ts3Client tempts3Client;
    public static string cookies;
    public static int playMode;
    public static string WangYiYunAPI_Address;
    public static string UNM_Address;
    List<long> playlist = new List<long>();
    public static int Playlocation = 0;
    private readonly SemaphoreSlim playlock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim Listeninglock = new SemaphoreSlim(1, 1);
    public void Initialize()
    {
        string iniFilePath;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("运行在Windows环境.");
            iniFilePath = "plugins/YunSettings.ini"; // Windows 文件目录
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string dockerEnvFilePath = "/.dockerenv";

            if (File.Exists(dockerEnvFilePath))
            {
                Console.WriteLine("运行在Docker环境.");
            }
            else
            {
                Console.WriteLine("运行在Linux环境.");
            }

            string location = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            iniFilePath = File.Exists(dockerEnvFilePath) ? location + "/data/plugins/YunSettings.ini" : location + "/plugins/YunSettings.ini";
        }
        else
        {
            throw new NotSupportedException("不支持的操作系统");
        }

        Console.WriteLine(iniFilePath);
        MyIni = new IniConfigSource(iniFilePath);

        playMode = int.TryParse(MyIni.Configs["YunBot"].Get("playMode"), out int playModeValue) ? playModeValue : 0;

        string cookiesValue = MyIni.Configs["YunBot"].Get("cookies1");
        cookies = string.IsNullOrEmpty(cookiesValue) ? "" : cookiesValue;

        string wangYiYunAPI_AddressValue = MyIni.Configs["YunBot"].Get("WangYiYunAPI_Address");
        WangYiYunAPI_Address = string.IsNullOrEmpty(wangYiYunAPI_AddressValue) ? "http://127.0.0.1:3000" : wangYiYunAPI_AddressValue;

        string unmAddressValue = MyIni.Configs["YunBot"].Get("UNM_Address");
        UNM_Address = string.IsNullOrEmpty(unmAddressValue) ? "" : unmAddressValue;

        Console.WriteLine(playMode);
        Console.WriteLine(cookies);
        Console.WriteLine(WangYiYunAPI_Address);
        Console.WriteLine(UNM_Address);

    }

    

    public void SetPlplayManager(PlayManager playManager)
    {
        tempplayManager = playManager;
    }
    public PlayManager GetplayManager()
    {
        return tempplayManager;
    }

    public InvokerData Getinvoker()
    {
        return tempinvoker;
    }

    public void SetInvoker(InvokerData invoker)
    {
        tempinvoker = invoker;
    }

    public void SetTs3Client(Ts3Client ts3Client)
    {
        tempts3Client = ts3Client;
    }

    public Ts3Client GetTs3Client()
    {
        return tempts3Client;
    }
    
    //===========================================初始化===========================================


    //===========================================播放模式===========================================
    [Command("yun mode")]
    public string Playmode(int mode)
    {
        if (mode >= 0 && mode <= 3)
        {
            playMode = mode;
            MyIni.Configs["YunBot"].Set("playMode", mode.ToString());
            MyIni.Save();
            return mode switch
            {
                0 => "顺序播放",
                1 => "顺序循环",
                2 => "随机播放",
                3 => "随机循环",
                _ => "未知播放模式",
            };
        }
        else
        {
            return "请输入正确的播放模式(0 到 3 之间的整数)";
        }
    }
    //===========================================播放模式===========================================


    //===========================================单曲播放===========================================
    [Command("yun play")]
    public async Task CommandYunPlay(string arguments, PlayManager playManager, InvokerData invoker, Ts3Client ts3Client)
    {
        //playlist.Clear();
        SetInvoker(invoker);
        SetPlplayManager(playManager);
        SetTs3Client(ts3Client);
        bool songFound = false;
        string arguments_1 = Uri.EscapeDataString(arguments);
        string urlSearch = $"{WangYiYunAPI_Address}/search?keywords={arguments_1}&limit=30";
        string searchJson = await HttpGetAsync(urlSearch);
        yunSearchSong yunSearchSong = JsonSerializer.Deserialize<yunSearchSong>(searchJson);
        //string[] splitArguments = arguments.Split(" ");
        //Console.WriteLine(splitArguments.Length);
        if(yunSearchSong.result.songs.Count != 0)
        {
            _ = ProcessSong(yunSearchSong.result.songs[0].id, ts3Client, playManager, invoker);
            songFound = true;
        }
        /*
        if (splitArguments.Length == 1)
        {
            _ = ProcessSong(yunSearchSong.result.songs[0].id, ts3Client, playManager, invoker);
            songFound = true;
        }

        else if (splitArguments.Length == 2)
        {
            // 歌曲名称和歌手
            string songName = splitArguments[0];
            string artist = splitArguments[1];

            for (int s = 0; s < yunSearchSong.result.songs.Count; s++)
            {
                if (yunSearchSong.result.songs[s].name == songName && yunSearchSong.result.songs[s].artists[0].name == artist)
                {
                    _ = ProcessSong(yunSearchSong.result.songs[s].id, ts3Client, playManager, invoker);
                    songFound = true;
                    break;
                }
            }
        }

        else
        {
            // 输入为空或格式不符合预期
            Console.WriteLine("请输入有效的歌曲信息");
            _ = ts3Client.SendChannelMessage("请输入有效的歌曲信息");
        }
        */
        Playlocation = songFound && Playlocation > 0 ? Playlocation - 1 : Playlocation;
        if (!songFound)
        {
            _ = ts3Client.SendChannelMessage("未找到歌曲");
        }
    }

    //===========================================单曲播放===========================================


    //===========================================添加到歌单=========================================
    [Command("yun add")]
    public async Task CommandYunAdd(string arguments, PlayManager playManager, InvokerData invoker, Ts3Client ts3Client, Player player)
    {
        SetInvoker(invoker);
        SetPlplayManager(playManager);
        SetTs3Client(ts3Client);
        bool songFound = false;
        string arguments_1 = Uri.EscapeDataString(arguments);
        string urlSearch = $"{WangYiYunAPI_Address}/search?keywords={arguments_1}&limit=30";
        string searchJson = await HttpGetAsync(urlSearch);
        yunSearchSong yunSearchSong = JsonSerializer.Deserialize<yunSearchSong>(searchJson);
        //string[] splitArguments = arguments.Split(" ");
        //Console.WriteLine(splitArguments.Length);
        if(yunSearchSong.result.songs.Count != 0)
        {
            playlist.Add(yunSearchSong.result.songs[0].id);
            Console.WriteLine(yunSearchSong.result.songs[0].id);
            songFound = true;
            _ = ts3Client.SendChannelMessage($"已将{yunSearchSong.result.songs[0].name}加入播放列表");
        }

        Playlocation = songFound && Playlocation > 0 ? Playlocation - 1 : Playlocation;
        if (!songFound)
        {
            //_ = ts3Client.SendChannelMessage("未找到歌曲");
            _ = ts3Client.SendChannelMessage("未找到歌曲");
            return;
        }
        
        if(!playManager.IsPlaying)
        {
            _ = ProcessSong(playlist[0], ts3Client, playManager, invoker);
        }
        await Listeninglock.WaitAsync();
        playManager.ResourceStopped += async (sender, e) => await SongPlayMode(playManager, invoker, ts3Client);
    }
    //===========================================添加到歌单=========================================

    //===========================================歌单播放===========================================
    [Command("yun gedan")]
    public async Task<string> CommandYunGedan(string arguments, PlayManager playManager, InvokerData invoker, Ts3Client ts3Client, Player player)
    {
        playlist.Clear();
        SetInvoker(invoker);
        SetPlplayManager(playManager);
        SetTs3Client(ts3Client);
        string arguments_1 = Uri.EscapeDataString(arguments);
        string urlSearch = $"{WangYiYunAPI_Address}/search?keywords={arguments_1}&type=1000&limit=10";
        string searchJson = await HttpGetAsync(urlSearch);
        PlaylistKeywordsSearch gedanDetail = JsonSerializer.Deserialize<PlaylistKeywordsSearch>(searchJson);
        if(gedanDetail == null || gedanDetail.result.playlists.Count == 0)
        {
            return "没有找到歌单，请检查输入";
        }
        string gedanid = gedanDetail.result.playlists[0].id.ToString();
        string gedanshuliang = gedanDetail.result.playlists[0].trackCount.ToString();
        Console.WriteLine($"gedanid={gedanid}, gedanshuliang={gedanshuliang}");
        _ = ts3Client.SendChannelMessage($"已找到歌单{gedanDetail.result.playlists[0].name}");
        _ = ts3Client.SendChannelMessage($"歌单共{gedanshuliang}首歌曲，正在添加到播放列表,请稍后。");

        int loopCount = -1;
        for (int i = 0; i < gedanDetail.result.playlists[0].trackCount; i += 50)
        {
            Console.WriteLine($"查询循环次数{loopCount+1}");
            loopCount += 1;
            if (i + 50 > gedanDetail.result.playlists[0].trackCount)
            {   
                // 如果歌单的歌曲数量小于50，那么查询的数量就是歌曲的数量，否则查询的数量就是歌曲的数量减去50乘以查询的次数
                i = gedanDetail.result.playlists[0].trackCount < 50 ? gedanDetail.result.playlists[0].trackCount : gedanDetail.result.playlists[0].trackCount - 50 * loopCount;
                // 构建查询URL，如果歌单的歌曲数量小于50，那么偏移量就是0，否则偏移量就是查询的数量
                int offset = gedanDetail.result.playlists[0].trackCount < 50 ? 0 : i;
                urlSearch = $"{WangYiYunAPI_Address}/playlist/track/all?id={gedanid}&limit=50&offset={offset}";
                searchJson = await HttpGetAsync(urlSearch);
                GeDan geDan1 = JsonSerializer.Deserialize<GeDan>(searchJson);
                for (int j = 0; j < i; j++){
                    playlist.Add(geDan1.songs[j].id);
                    Console.WriteLine(geDan1.songs[j].id);
                }
                break;
            }
            urlSearch = $"{WangYiYunAPI_Address}/playlist/track/all?id={gedanid}&limit=50&offset={i}";
            searchJson = await HttpGetAsync(urlSearch);
            GeDan geDan = JsonSerializer.Deserialize<GeDan>(searchJson);
            for (int j = 0; j < 50; j++){
                playlist.Add(geDan.songs[j].id);
                Console.WriteLine(geDan.songs[j].id);
            }
        }

        Playlocation = 0;
        _ = ProcessSong(playlist[0], ts3Client, playManager, invoker);
        Console.WriteLine($"歌单共{playlist.Count}首歌");
        await Listeninglock.WaitAsync();
        playManager.ResourceStopped += async (sender, e) => await SongPlayMode(playManager, invoker, ts3Client);
        return $"播放列表加载完成,已加载{playlist.Count}首歌";
    }
    //===========================================歌单播放===========================================


    //===========================================歌单id播放===========================================
    [Command("yun gedanid")]
    public async Task<string> CommandYunGedanId(string arguments, PlayManager playManager, InvokerData invoker, Ts3Client ts3Client, Player player)
    {
        playlist.Clear();
        SetInvoker(invoker);
        SetPlplayManager(playManager);
        SetTs3Client(ts3Client);
        string urlSearch = $"{WangYiYunAPI_Address}/playlist/detail?id={arguments}";
        string searchJson = await HttpGetAsync(urlSearch);
        GedanDetail gedanDetail = JsonSerializer.Deserialize<GedanDetail>(searchJson);
        string gedanshuliang = gedanDetail.playlist.trackCount.ToString();
        _ = ts3Client.SendChannelMessage($"歌单共{gedanshuliang}首歌曲，正在添加到播放列表,请稍后。");
        int loopCount = -1;
        for (int i = 0; i < gedanDetail.playlist.trackCount; i += 50)
        {
            Console.WriteLine($"查询循环次数{loopCount+1}");
            loopCount += 1;
            if (i + 50 > gedanDetail.playlist.trackCount)
            {   
                // 如果歌单的歌曲数量小于50，那么查询的数量就是歌曲的数量，否则查询的数量就是歌曲的数量减去50乘以查询的次数
                i = gedanDetail.playlist.trackCount < 50 ? gedanDetail.playlist.trackCount : gedanDetail.playlist.trackCount - 50 * loopCount;
                // 构建查询URL，如果歌单的歌曲数量小于50，那么偏移量就是0，否则偏移量就是查询的数量
                int offset = gedanDetail.playlist.trackCount < 50 ? 0 : i;
                urlSearch = $"{WangYiYunAPI_Address}/playlist/track/all?id={arguments}&limit=50&offset={offset}";
                searchJson = await HttpGetAsync(urlSearch);
                GeDan geDan1 = JsonSerializer.Deserialize<GeDan>(searchJson);
                for (int j = 0; j < i; j++){
                    playlist.Add(geDan1.songs[j].id);
                    Console.WriteLine(geDan1.songs[j].id);
                }
                break;
            }
            urlSearch = $"{WangYiYunAPI_Address}/playlist/track/all?id={arguments}&limit=50&offset={i}";
            searchJson = await HttpGetAsync(urlSearch);
            GeDan geDan = JsonSerializer.Deserialize<GeDan>(searchJson);
            for (int j = 0; j < 50; j++){
                playlist.Add(geDan.songs[j].id);
                Console.WriteLine(geDan.songs[j].id);
            }
        }
        Playlocation = 0;
        _ = ProcessSong(playlist[0], ts3Client, playManager, invoker);
        Console.WriteLine($"歌单共{playlist.Count}首歌");
        await Listeninglock.WaitAsync();
        playManager.ResourceStopped += async (sender, e) => await SongPlayMode(playManager, invoker, ts3Client);
        return $"播放列表加载完成,已加载{playlist.Count}首歌";
    }
    //===========================================歌单id播放===========================================

    //===========================================下一曲===========================================
    [Command("yun next")]
    public async Task CommandYunNext(PlayManager playManager, InvokerData invoker, Ts3Client ts3Client)
    {
        await SongPlayMode(playManager, invoker, ts3Client);
    }
    //===========================================下一曲=============================================
    [Command("yun stop")]
    public async Task CommandYunStop(PlayManager playManager, Ts3Client ts3Client)
    {
        playlist.Clear();
        await playManager.Stop();
    }

    //===========================================播放逻辑===========================================
    private async Task SongPlayMode(PlayManager playManager, InvokerData invoker, Ts3Client ts3Client)
    {
        try
        {
            switch (playMode)
            {
                case 0: //顺序播放
                    Playlocation += 1;
                    await ProcessSong(playlist[Playlocation], ts3Client, playManager, invoker);
                    break;
                case 1:  //顺序循环
                    if (Playlocation == playlist.Count - 1)
                    {
                        Playlocation = 0;
                        await ProcessSong(playlist[Playlocation], ts3Client, playManager, invoker);
                    }
                    else
                    {
                        Playlocation += 1;
                        await ProcessSong(playlist[Playlocation], ts3Client, playManager, invoker);
                    }
                    break;
                case 2:  //随机播放
                    Random random = new Random();
                    Playlocation = random.Next(0, playlist.Count);
                    await ProcessSong(playlist[Playlocation], ts3Client, playManager, invoker);
                    break;
                case 3:  //随机循环
                    Random random1 = new Random();
                    Playlocation = random1.Next(0, playlist.Count);
                    await ProcessSong(playlist[Playlocation], ts3Client, playManager, invoker);
                    break;
                default:
                    break;
            } 
        }
        catch (Exception)
        {
            Console.WriteLine("播放列表已空");
            _ = ts3Client.SendChannelMessage("已停止播放");
        }
    }
    private async Task ProcessSong(long id, Ts3Client ts3Client, PlayManager playManager, InvokerData invoker)
    {
        await playlock.WaitAsync();
        try {
            long musicId = id;
            string musicCheckUrl = $"{WangYiYunAPI_Address}/check/music?id={musicId}";
            string searchMusicCheckJson = await HttpGetAsync(musicCheckUrl);
            MusicCheck musicCheckJson = JsonSerializer.Deserialize<MusicCheck>(searchMusicCheckJson);

            // 根据音乐检查结果获取音乐播放URL
            string musicUrl = musicCheckJson.success.ToString() == "False" ? await GetcheckMusicUrl(musicId, true) : await GetMusicUrl(musicId, true);

            // 构造获取音乐详情的URL
            string musicDetailUrl = $"{WangYiYunAPI_Address}/song/detail?ids={musicId}";
            string musicDetailJson = await HttpGetAsync(musicDetailUrl);
            MusicDetail musicDetail = JsonSerializer.Deserialize<MusicDetail>(musicDetailJson);

            // 从音乐详情中获取音乐图片URL和音乐名称
            string musicImgUrl = musicDetail.songs[0].al.picUrl;
            string musicName = musicDetail.songs[0].name;
            Console.WriteLine($"歌曲id：{musicId}，歌曲名称：{musicName}，版权：{musicCheckJson.success}");

            // 设置Bot的头像为音乐图片
            _ = MainCommands.CommandBotAvatarSet(ts3Client, musicImgUrl);

            // 设置Bot的描述为音乐名称
            _ = MainCommands.CommandBotDescriptionSet(ts3Client, musicName);

            // 在控制台输出音乐播放URL
            Console.WriteLine(musicUrl);

            // 如果音乐播放URL不是错误，则添加到播放列表并通知频道
            if (musicUrl != "error")
            {
                _ = MainCommands.CommandPlay(playManager, invoker, musicUrl);

                // 更新Bot的描述为当前播放的音乐名称
                _ = MainCommands.CommandBotDescriptionSet(ts3Client, musicName);

                // 发送消息到频道，通知正在播放的音乐
                if (playlist.Count == 0)
                {
                    _ = ts3Client.SendChannelMessage($"正在播放：{musicName}");
                }
                else
                {
                    _ = ts3Client.SendChannelMessage($"正在播放第{Playlocation+1}首：{musicName}");
                }
            }
        }
        finally
        {
            playlock.Release();
        }
    }
    //===========================================播放逻辑===========================================

    //===========================================登录部分===========================================
    [Command("yun login")]
    public static async Task<string> CommandLoginAsync(Ts3Client ts3Client, TsFullClient tsClient)
    {
        // 获取登录二维码的 key
        string key = await GetLoginKey();

        // 生成登录二维码并获取二维码图片的 base64 字符串
        string base64String = await GetLoginQRImage(key);

        // 发送二维码图片到 TeamSpeak 服务器频道
        await ts3Client.SendChannelMessage("正在生成二维码");
        await ts3Client.SendChannelMessage(base64String);

        // 将 base64 字符串转换为二进制图片数据，上传到 TeamSpeak 服务器作为头像
        await UploadQRImage(tsClient, base64String);

        // 设置 TeamSpeak 服务器的描述信息
        await ts3Client.ChangeDescription("请用网易云APP扫描二维码登陆");

        int i = 0;
        long code;
        string result;

        while (true)
        {
            // 检查登录状态
            Status1 status = await CheckLoginStatus(key);

            code = status.code;
            cookies = status.cookie;
            i = i + 1;
            Thread.Sleep(1000);

            if (i == 120)
            {
                result = "登陆失败或者超时";
                await ts3Client.SendChannelMessage("登陆失败或者超时");
                break;
            }

            if (code == 803)
            {
                result = "登陆成功";
                await ts3Client.SendChannelMessage("登陆成功");
                break;
            }
        }

        // 登录完成后删除上传的头像
        _ = await tsClient.DeleteAvatar();

        // 更新 cookies 到配置文件
        MyIni.Configs["YunBot"].Set("cookies1", $"\"{cookies}\"");
        MyIni.Save();

        return result;
    }

    // 获取登录二维码的 key
    private static async Task<string> GetLoginKey()
    {
        string url = WangYiYunAPI_Address + "/login/qr/key" + "?timestamp=" + GetTimeStamp();
        string json = await HttpGetAsync(url);
        LoginKey loginKey = JsonSerializer.Deserialize<LoginKey>(json);
        return loginKey.data.unikey;
    }

    // 生成登录二维码并获取二维码图片的 base64 字符串
    private static async Task<string> GetLoginQRImage(string key)
    {
        string url = WangYiYunAPI_Address + $"/login/qr/create?key={key}&qrimg=true&timestamp={GetTimeStamp()}";
        string json = await HttpGetAsync(url);
        LoginImg loginImg = JsonSerializer.Deserialize<LoginImg>(json);
        return loginImg.data.qrimg;
    }

    // 上传二维码图片到 TeamSpeak 服务器
    private static async Task UploadQRImage(TsFullClient tsClient, string base64String)
    {
        string[] img = base64String.Split(",");
        byte[] bytes = Convert.FromBase64String(img[1]);
        Stream stream = new MemoryStream(bytes);
        _ = await tsClient.UploadAvatar(stream);
    }

    // 检查登录状态
    private static async Task<Status1> CheckLoginStatus(string key)
    {
        string url = WangYiYunAPI_Address + $"/login/qr/check?key={key}&timestamp={GetTimeStamp()}";
        string json = await HttpGetAsync(url);
        Status1 status = JsonSerializer.Deserialize<Status1>(json);
        Console.WriteLine(json);
        return status;
    }
    //===============================================登录部分===============================================


    //===============================================获取歌曲信息===============================================
    //以下全是功能性函数
    public static async Task<string> GetMusicUrl(long id, bool usingCookie = false)
    {
        return await GetMusicUrl(id.ToString(), usingCookie);
    }

    public static async Task<string> GetMusicUrl(string id, bool usingCookie = false)
    {
        string url = $"{WangYiYunAPI_Address}/song/url?id={id}";
        if (usingCookie && !string.IsNullOrEmpty(cookies))
        {
            url += $"&cookie={cookies}";
        }

        string musicUrlJson = await HttpGetAsync(url);
        musicURL musicUrl = JsonSerializer.Deserialize<musicURL>(musicUrlJson);

        if (musicUrl.code != 200)
        {
            // 处理错误情况，这里你可以根据实际情况进行适当的处理
            return string.Empty;
        }

        string mp3 = musicUrl.data[0].url;
        return mp3;
    }

    public static async Task<string> GetcheckMusicUrl(long id, bool usingcookie = false) //获得无版权歌曲URL
    {
        string url;
        url = WangYiYunAPI_Address + "/song/url?id=" + id.ToString() + "&proxy=" + UNM_Address;
        string musicurljson = await HttpGetAsync(url);
        musicURL musicurl = JsonSerializer.Deserialize<musicURL>(musicurljson);
        string mp3 = musicurl.data[0].url.ToString();
        string checkmp3 = mp3.Replace("http://music.163.com", UNM_Address);
        return checkmp3;
    }

    public static async Task<string> GetMusicName(string arguments)//获得歌曲名称
    {
        string musicdetailurl = WangYiYunAPI_Address + "/song/detail?ids=" + arguments;
        string musicdetailjson = await HttpGetAsync(musicdetailurl);
        MusicDetail musicDetail = JsonSerializer.Deserialize<MusicDetail>(musicdetailjson);
        string musicname = musicDetail.songs[0].name;
        return musicname;
    }
    //===============================================获取歌曲信息===============================================



    //===============================================HTTP相关===============================================
    public static async Task<string> HttpGetAsync(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.Accept = "text/html, application/xhtml+xml, */*";
        request.ContentType = "application/json";

        // 异步获取响应
        using HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
        // 异步读取响应流
        using StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public static string GetTimeStamp() //获得时间戳
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
    //===============================================HTTP相关===============================================
    public void Dispose()
    {
        
    }
}
