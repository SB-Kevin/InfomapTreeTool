using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TablesService;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public string DBConString = string.Empty;
        public string StorageConString = string.Empty;
        public List<Skill> SkillList = null;
        public List<CompassNodes> SkillNodes = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton3.Select();
            SkillList = GetSkillList();
            
            treeView1.CheckBoxes = false;
           
            treeView1.ShowLines = true;


        }
        private void FocusOnRoot()
        {
            this.treeView1.SelectedNode = this.treeView1.Nodes[0];
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var mappingTarget = SkillNodes?.Where(x => x.Node_GUID.Equals(e.Node.Name))?.FirstOrDefault();
            if (mappingTarget != null)
            {
                sb.AppendLine($"Order：{mappingTarget.Node_Name_Order_Number}");
           
                sb.AppendLine($"Guid：{e.Node.Name}");

                sb.AppendLine($"Text：{e.Node.Text}");
                
                sb.AppendLine($"ServiceTypeID：{mappingTarget.Service_TypeID}");
                sb.AppendLine($"FunctionTypeId：{mappingTarget.Function_TypeID}");
                
                string Parent = e.Node.Parent == null ? "Root" : e.Node.Parent.Text;
                sb.AppendLine($"ParentName：{Parent}");
                
                sb.AppendLine($"ChildNodeCount：{e.Node.GetNodeCount(false)}");
                
                sb.AppendLine($"FullPath：{e.Node.FullPath}");
                
                sb.AppendLine($"FullPath：{e.Node.Level}");

            }
            else
            {
                sb.AppendLine($"Index：{e.Node.Index}");
               
                sb.AppendLine($"Name：{e.Node.Name}");
                
                sb.AppendLine($"Text：{e.Node.Text}");
                
                sb.AppendLine($"Tag：{e.Node.Tag}");
                
                string Parent = e.Node.Parent == null ? "Root" : e.Node.Parent.Text;
                sb.AppendLine($"Parent：{Parent}");
                
                sb.AppendLine($"Count：{e.Node.GetNodeCount(false)}");
                
                sb.AppendLine($"FullPath：{e.Node.FullPath}");
               
                sb.AppendLine($"FullPath：{e.Node.Level}");
            }
            txtNodeInfo.Text = sb.ToString();
        }

        private void TreeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode ChildeNode in e.Node.Nodes)
            {
                ChildeNode.Checked = e.Node.Checked;
            }
        }
        private int IDColIndex = 0;
        private int ParentIDColIndex = 1;
        private int TextColIndex = 2;

        private bool TreeBuild(DataTable dt)
        {
            if (TreeRootExist(dt))
            {
                treeView1.Nodes.Clear();
                CreateRootNode(this.treeView1, dt);
                return true;
            }
            return false;
        }

        private bool TreeRootExist(DataTable dt)
        {
            EnumerableRowCollection<DataRow> result = dt
                .AsEnumerable()
                .Where(r => r.Field<string>(this.ParentIDColIndex) == new Guid("12345678-9012-3456-7890-123456789012").ToString());

            if (result.Any() == false)
            {
                MessageBox.Show("沒有 Root 節點資料，無法建立 TreeView");
                return false;
            }

            if (result.Count() > 1)
            {
                MessageBox.Show("Root 節點超過 1 個，無法建立 TreeView");
                return false;
            }
            return true;
        }

        private DataRow GetTreeRoot(DataTable dt)
        {
            return dt.AsEnumerable()
                .Where(r => r.Field<string>(this.ParentIDColIndex) == new Guid("12345678-9012-3456-7890-123456789012").ToString())
                .First();
        }

        private IEnumerable<DataRow> GetTreeNodes(DataTable dt, TreeNode Node)
        {
            return dt.AsEnumerable()
                .Where(r => r.Field<string>(this.ParentIDColIndex) == Node.Name)
                .OrderBy(r => r.Field<string>(this.IDColIndex));
        }

        private void CreateRootNode(TreeView tree, DataTable dt)
        {
            DataRow Root = GetTreeRoot(dt);
            TreeNode Node = new TreeNode();
            Node.Text = Root.Field<string>(this.TextColIndex);
            Node.Name = Root.Field<string>(this.IDColIndex);
            tree.Nodes.Add(Node);

            CreateNode(tree, dt, Node);
        }

        private void CreateNode(TreeView tree, DataTable dt, TreeNode Node)
        {
            IEnumerable<DataRow> Rows = GetTreeNodes(dt, Node);

            TreeNode NewNode;
            foreach (DataRow r in Rows)
            {
                NewNode = new TreeNode();
                NewNode.Name = r.Field<string>(this.IDColIndex);
                NewNode.Text = r.Field<string>(this.TextColIndex);
                Node.Nodes.Add(NewNode);

                CreateNode(tree, dt, NewNode);
            }
        }

        private DataTable GetTreeData(string skillGuid)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("DepID", typeof(string));
            dt.Columns.Add("ParentID", typeof(string));
            dt.Columns.Add("DepName", typeof(string));

            List<CompassNodes> SkillNodes = GetInfomapNodes(skillGuid);
            foreach (var skillNode in SkillNodes)
            {
                dt.Rows.Add(skillNode.Node_GUID, skillNode.Parent_GUID, skillNode.Node_Name);

            }

            return dt;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            DBConString = string.Empty; //<Your Product DB ConnectionString>
            StorageConString = string.Empty; //<Your Product DB ConnectionString>

            SkillList = GetSkillList();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            DBConString = string.Empty; //<Your UAT DB ConnectionString>
            StorageConString = string.Empty; //<Your UAT DB ConnectionString>

            SkillList = GetSkillList();
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            DBConString = string.Empty; //<Your DEV DB ConnectionString>
            StorageConString = string.Empty; //<Your DEV DB ConnectionString>

            SkillList = GetSkillList();
        }

        private List<CompassNodes> GetInfomapNodes(string skillGuid)
        {
            var CompassNodesTable = new TablesHelper<CompassNodes>().CreateTablesHelper(StorageConString, "MapDataCompassNodes");

            return CompassNodesTable.SelectBySingleProperty("PartitionKey", skillGuid);
        }

        private List<Skill> GetSkillList()
        {
            CB_SkillList.Items.Clear();
            List<Skill> result = new List<Skill>();
            using (SqlConnection con = new SqlConnection(DBConString))
            {
                string selectSql = $"select [SkillGuid],[SkillName] from [Skill] where [SkillMappingEngineName] = 'InfomapAgent'";
                using (SqlCommand com = new SqlCommand(selectSql, con))
                {
                    con.Open();
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Skill()
                            {
                                SkillGuid = (reader["SkillGuid"].ToString()),
                                SkillName = (reader["SkillName"].ToString())
                            }); ;
                        }
                    }
                }
            }
            CB_SkillList.Items.AddRange(result.Select(x => x.SkillName).ToArray());
            return result;
        }

        private void Btn_GetTree_Click(object sender, EventArgs e)
        {
            string skillGuid = string.Empty;
            try
            {
                string selectSkill = CB_SkillList.SelectedItem.ToString();
                skillGuid = SkillList.Where(x => x.SkillName.Equals(selectSkill))?.FirstOrDefault()?.SkillGuid;
            }
            catch { }
            if (string.IsNullOrEmpty(textBox1.Text))
                textBox1.Text = skillGuid;

            skillGuid = textBox1.Text;


            DataTable dt = GetTreeData(skillGuid);
            if (TreeBuild(dt))
            {
                treeView1.ExpandAll();
                treeView1.AfterSelect += TreeView1_AfterSelect;
                treeView1.AfterCheck += TreeView1_AfterCheck;

                FocusOnRoot();
            }
        }
    }

    public class Skill
    {
        public string SkillGuid { get; set; }
        public string SkillName { get; set; }

    }
}
