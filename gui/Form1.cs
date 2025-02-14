using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace gui
{
    public partial class Form1 : Form
    {
        string version_select;
        string mod_loader_select;
        string jvm_arg;
        string final_command;
        string version_modloader_select;

        public Form1()
        {
            InitializeComponent();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 100;
            toolTip1.ReshowDelay = 200;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(version_cheak, "версии обновляются автомотически");
            toolTip1.SetToolTip(version, "свою версию или сборку надо писать без пробелов и тере");
            toolTip1.SetToolTip(mod_loader, "для optifine можно использовать optifine для forge или fabric");
            toolTip1.SetToolTip(gb, "число оперативки может быть только целым в гб");
        }

        // Метод для получения UUID пользователя Minecraft
        static string GetMinecraftUUID(string username)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(username);
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(nameBytes);
                return new Guid(hash).ToString();
            }
        }

        // Метод для создания и запуска bat файла
        static void CreateAndRunBatFile(string final_command)
        {
            string fullCommand = "cd.. & cd.. & cd.. & cd portablemc-main & " + final_command + "& exit";
            string batFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runCommands.bat");
            File.WriteAllText(batFilePath, fullCommand);
            Thread.Sleep(100);
            Process.Start(batFilePath);
            Thread.Sleep(2000);
            Application.Exit();
        }

        // Метод для получения версий Minecraft с фильтрацией
        private const string Url = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public async Task<List<string>> GetFilteredMinecraftVersionsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "request");
                var response = await client.GetStringAsync(Url);
                var versionManifest = JsonConvert.DeserializeObject<VersionManifest>(response);

                return versionManifest.Versions
                    .Where(v => v.Type == "release" && string.Compare(v.Id, "1.12.2") >= 0)
                    .Select(v => v.Id)
                    .ToList();
            }
        }
        static string GetMinecraftPath()
        {
            // Получаем текущую директорию
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Получаем путь до родительской директории, два уровня вверх, и убираем "gui"
            string baseDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\"));

            // Строим новый путь к minecraft
            string minecraftPath = Path.Combine(baseDirectory, "portablemc-main", "minecraft");

            return minecraftPath;
        }

        // Загрузка версий в listBox1
        private async void Form1_Load(object sender, EventArgs e)
        {
            var fetcher = new MinecraftVersionFetcher();
            var versions = await fetcher.GetFilteredMinecraftVersionsAsync();

            listBox1.Items.Clear();
            foreach (var version in versions)
            {
                listBox1.Items.Add(version);
            }
        }

        // Обработчик кнопки
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == " ")
            {
                version_select = listBox1.Text;
            }
            else
            {
                version_select = textBox1.Text;
            }

            // Добавляем параметр для максимального объема памяти
            if (checkBox1.Checked == true)
            {
                jvm_arg = $"-Xmx{textBox3.Text}G " +
                    "-Xms2G -XX:+UseG1GC -XX:+UnlockExperimentalVMOptions -XX:G1NewSizePercent=30 " +
                    "-XX:G1MaxNewSizePercent=40 -XX:G1HeapRegionSize=8M -XX:G1ReservePercent=20 " +
                    "-XX:MaxGCPauseMillis=50 -XX:InitiatingHeapOccupancyPercent=15 -XX:+ParallelRefProcEnabled " +
                    "-XX:+AlwaysPreTouch -XX:+DisableExplicitGC -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem " +
                    "-XX:+OptimizeStringConcat -XX:+UseStringDeduplication";
            }
            else
            {
                jvm_arg = $"-Xmx{textBox3.Text}G";
            }

            if (listBox2.SelectedItem.ToString() == "NoModLoader")
            {
                version_modloader_select = version_select;
            }
            else
            {
                version_modloader_select = listBox2.SelectedItem + ":" + version_select;
            }

            // Формируем финальную команду
            final_command = $@".\pypoetry\venv\Scripts\poetry.exe run portablemc --main-dir ""{GetMinecraftPath()}"" --work-dir ""{GetMinecraftPath()}"" start ""--jvm-args={jvm_arg}"" {version_modloader_select.ToLower()} -u {textBox2.Text} -i {GetMinecraftUUID(textBox2.Text)}";
            Clipboard.SetText(version_modloader_select);
            CreateAndRunBatFile(final_command);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fullCommand = "cd.. & cd.. & cd.. & cd portablemc-main & cd minecraft & explorer .& exit";
            string batFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "runCommandsExplorer.bat");
            File.WriteAllText(batFilePath, fullCommand);
            Thread.Sleep(10);
            Process.Start(batFilePath);
        }
    }

    // Классы для парсинга JSON
    public class VersionManifest
    {
        public List<VersionInfo> Versions { get; set; }
    }

    public class VersionInfo
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Time { get; set; }
        public string ReleaseTime { get; set; }
    }

    public class MinecraftVersionFetcher
    {
        private const string VERSION_MANIFEST_URL = "https://launchermeta.mojang.com/mc/game/version_manifest.json";

        public async Task<List<string>> GetFilteredMinecraftVersionsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "request");
                var response = await client.GetStringAsync(VERSION_MANIFEST_URL);
                var versionManifest = JsonConvert.DeserializeObject<VersionManifest>(response);

                return versionManifest.Versions
                    .Where(v => v.Type == "release" && string.Compare(v.Id, "0.0.0") >= 0)
                    .Select(v => v.Id)
                    .ToList();
            }
        }
    }
}
