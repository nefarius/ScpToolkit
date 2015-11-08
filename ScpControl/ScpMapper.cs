using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using log4net;
using ScpControl.ScpCore;

namespace ScpControl
{
    [Obsolete]
    public sealed partial class ScpMapper : Component
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string m_FileName = "ScpMapper.xml";

        private static string m_FilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" +
                                             m_FileName;

        private DateTime m_Last = DateTime.Now;
        private XmlDocument m_Map = new XmlDocument();
        private bool m_Started;

        private FileSystemWatcher m_Watcher =
            new FileSystemWatcher(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), m_FileName);

        public ScpMapper()
        {
            InitializeComponent();
        }

        public ScpMapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public bool Started
        {
            get { return m_Started; }
        }

        public string[] Profiles
        {
            get { return userMapper.Profiles; }
        }

        public string Active
        {
            get { return userMapper.Active; }
            set
            {
                new Thread(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(m_Map.InnerXml))
                            return;

                        m_Map.SelectSingleNode("/ScpMapper/Active").FirstChild.Value = value;
                        m_Map.Save(m_FilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Unexpected error: {0}", ex);
                    }
                }).Start();
            }
        }

        public string Xml
        {
            get { return m_Map.InnerXml; }
            set
            {
                new Thread(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(value))
                            return;

                        m_Map.LoadXml(value);
                        m_Map.Save(m_FilePath);
                    }
                    catch (XmlException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("Unexpected error: {0}", ex);
                    }
                }).Start();
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            Start();
            m_Last = DateTime.Now;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if ((DateTime.Now - m_Last).TotalMilliseconds < 100) return;

            Thread.Sleep(1000);
            Reload();
            m_Last = DateTime.Now;
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Stop();
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (e.Name == m_FilePath)
            {
                Thread.Sleep(1000);
                Start();
                m_Last = DateTime.Now;
            }
            else
            {
                Stop();
            }
        }

        public bool Open()
        {
            m_Watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName |
                                     NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                                     NotifyFilters.Security | NotifyFilters.Size;

            m_Watcher.Created += OnCreated;
            m_Watcher.Changed += OnChanged;
            m_Watcher.Deleted += OnDeleted;
            m_Watcher.Renamed += OnRenamed;

            return true;
        }

        public bool Start()
        {
            if (!m_Started)
            {
                try
                {
                    m_Watcher.EnableRaisingEvents = true;

                    m_Map = new XmlDocument();

                    m_Map.Load(m_FilePath);

                    m_Watcher.EnableRaisingEvents = m_Started = userMapper.Initialize(m_Map);
                }
                catch (Exception e)
                {
                    Log.DebugFormat("-- Mapper.Start  [{0}]", e.Message);
                }
            }

            return m_Started;
        }

        public bool Stop()
        {
            if (m_Started)
            {
                try
                {
                    m_Started = false;

                    userMapper.Shutdown();
                }
                catch (Exception e)
                {
                    Log.DebugFormat("-- Mapper.Stop   [{0}]", e.Message);
                }
            }

            return !m_Started;
        }

        public bool Close()
        {
            m_Watcher.EnableRaisingEvents = false;

            m_Watcher.Created -= OnCreated;
            m_Watcher.Changed -= OnChanged;
            m_Watcher.Deleted -= OnDeleted;
            m_Watcher.Renamed -= OnRenamed;

            return true;
        }

        public bool Reload()
        {
            var Updated = false;

            if (m_Started)
            {
                try
                {
                    m_Map = new XmlDocument();

                    m_Map.Load(m_FilePath);

                    m_Watcher.EnableRaisingEvents = Updated = userMapper.Initialize(m_Map);
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("-- Mapper.Reload [{0}]", e.Message);
                }
            }

            return Updated;
        }

        public bool Remap(DsModel Type, int PadId, string MacAddr, byte[] Input, byte[] Output)
        {
            var Mapped = false;

            if (m_Started)
            {
                try
                {
                    Mapped = userMapper.Remap(Type, PadId, MacAddr, Input, Output);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Unexpected error in Remap: {0}", ex);
                }
            }

            return Mapped;
        }
    }
}