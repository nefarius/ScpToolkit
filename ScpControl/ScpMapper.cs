using System;
using System.ComponentModel;

using System.IO;
using System.Xml;
using System.Reflection;
using System.Threading;

namespace ScpControl
{
    public partial class ScpMapper : Component
    {
        protected XmlDocument   m_Map      = new XmlDocument();
        protected static String m_FileName = "ScpMapper.xml";
        protected static String m_FilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + m_FileName;

        protected FileSystemWatcher m_Watcher = new FileSystemWatcher(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), m_FileName);
        protected DateTime m_Last = DateTime.Now;


        protected Boolean m_Started = false;
        public Boolean Started 
        {
            get { return m_Started; }
        }

        public event EventHandler<DebugEventArgs> Debug = null;

        protected virtual void LogDebug(String Data) 
        {
            DebugEventArgs args = new DebugEventArgs(Data);

            if (Debug != null)
            {
                Debug(this, args);
            }
        }

        protected virtual void OnDebug(object sender, DebugEventArgs e) 
        {
            if (Debug != null)
            {
                Debug(this, e);
            }
        }


        protected void OnCreated(object sender, FileSystemEventArgs e) 
        {
            Thread.Sleep(1000);  Start();
            m_Last = DateTime.Now;
        }

        protected void OnChanged(object sender, FileSystemEventArgs e) 
        {
            if ((DateTime.Now - m_Last).TotalMilliseconds < 100) return;

            Thread.Sleep(1000); Reload();
            m_Last = DateTime.Now;
        }

        protected void OnDeleted(object sender, FileSystemEventArgs e) 
        {
            Stop();
       }

        protected void OnRenamed(object sender, RenamedEventArgs e)    
        {
            if (e.Name == m_FilePath)
            {
                Thread.Sleep(1000); Start();
                m_Last = DateTime.Now;
            }
            else
            {
                Stop();
            }
        }


        public ScpMapper() 
        {
            InitializeComponent();
        }

        public ScpMapper(IContainer container) 
        {
            container.Add(this);

            InitializeComponent();
        }


        public virtual Boolean Open()  
        {
            m_Watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;

            m_Watcher.Created += OnCreated;
            m_Watcher.Changed += OnChanged;
            m_Watcher.Deleted += OnDeleted;
            m_Watcher.Renamed += OnRenamed;

            return true;
        }

        public virtual Boolean Start() 
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
                catch (Exception e) { LogDebug(String.Format("-- Mapper.Start  [{0}]", e.Message)); }
            }

            return m_Started;
        }

        public virtual Boolean Stop()  
        {
            if (m_Started)
            {
                try
                {
                    m_Started = false;

                    userMapper.Shutdown();
                }
                catch (Exception e) { LogDebug(String.Format("-- Mapper.Stop   [{0}]", e.Message)); }
            }

            return !m_Started;
        }

        public virtual Boolean Close() 
        {
            m_Watcher.EnableRaisingEvents = false;

            m_Watcher.Created -= OnCreated;
            m_Watcher.Changed -= OnChanged;
            m_Watcher.Deleted -= OnDeleted;
            m_Watcher.Renamed -= OnRenamed;

            return true;
        }


        public virtual Boolean Reload() 
        {
            Boolean Updated = false;

            if (m_Started)
            {
                try
                {
                    m_Map = new XmlDocument();

                    m_Map.Load(m_FilePath);

                    m_Watcher.EnableRaisingEvents = Updated = userMapper.Initialize(m_Map);
                }
                catch (Exception e) { LogDebug(String.Format("-- Mapper.Reload [{0}]", e.Message)); }
            }

            return Updated;
        }


        public virtual Boolean Remap(DsModel Type, Int32 PadId, String MacAddr, Byte[] Input, Byte[] Output) 
        {
            Boolean Mapped = false;

            if (m_Started)
            {
                try
                {
                    Mapped = userMapper.Remap(Type, PadId, MacAddr, Input, Output);
                }
                catch { }
            }

            return Mapped;
        }


        public virtual String[] Profiles 
        {
            get { return userMapper.Profiles; }
        }

        public virtual String Active 
        {
            get { return userMapper.Active; }
            set 
            {
                new Thread(() =>
                {
                    try
                    {
                        m_Map.SelectSingleNode("/ScpMapper/Active").FirstChild.Value = value;
                        m_Map.Save(m_FilePath);
                    }
                    catch { }
                }).Start();
            }
        }

        public virtual String Xml 
        {
            get { return m_Map.InnerXml; }
            set 
            {
                new Thread(() =>
                {
                    try
                    {
                        m_Map.LoadXml(value);
                        m_Map.Save(m_FilePath);
                    }
                    catch { }
                }).Start();
            }
        }
    }
}
