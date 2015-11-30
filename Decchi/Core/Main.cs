using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PublishingModule.Twitter;

namespace Decchi
{
    /// <summary>
    /// 뎃찌의 메인 폼을 정의합니다.
    /// </summary>
    public partial class Main : Form
    {
        public Main()
        {
            // 폼에 트위터 유저 정보 매핑
            var me = TwitterCommunicator.Instance.Me;
            if (me != null)
            {
                InitializeComponent();

                Task.Factory.StartNew( delegate // <-- 륜님 이거 줮나 편하네요 ㅇ.<
                {
                    var img = Globals.GetImageFromUrl(me.ProfileImageUrl.Replace("_normal", ""));
					this.Invoke(new Action(delegate { picbox_profileImage.BackgroundImage = img }));
                });
                var nameStr = me.Name;

                // 이름 사이에 한칸씩 여백을 주면 멋지지 않을까 해서 만들어 봤는데,
                // 적용시켜보니 생각보다 별로라 지울까 했지만,
                // 왠지 지우자니 아까워서 여기에 남기다.
                //var sourceLength = nameStr.Length;
                //for (int i = sourceLength - 1; i >= 0; i--)
                //{
                //    nameStr = nameStr.Insert(i, " ");
                //}

                label_UserName.Text = nameStr;
                label_ScreenName.Text = me.ScreenName;

                PlaceControls();
            }
            else
            {
                MessageBox.Show("유저 정보를 받아오는데 실패했습니다.", "네트워크 오류");
                this.Close();
            }
        }

        /// <summary>
        /// 중앙을 기준으로 컨트롤을 배치합니다.
        /// </summary>
        public void PlaceControls()
        {
            var center = new Point(this.Width / 2, this.Height / 2);
            picbox_profileImage.Location = Point.Subtract(center, new Size(picbox_profileImage.Width / 2, picbox_profileImage.Height / 2 + 150));
            label_UserName.Location = Point.Subtract(center, new Size(label_UserName.Width / 2, label_UserName.Height / 2 + 35));
            label_ScreenName.Location = Point.Subtract(center, new Size(label_ScreenName.Width / 2, label_ScreenName.Height / 2 + 5));
			btn_post.Location = Point.Subtract(center, new Size(btn_post.Width / 2, btn_post.Height / 2 - 150));
		}

        #region 폼 이벤트

        private void Main_Resize(object sender, EventArgs e)
        {
            PlaceControls();
        }

		private void btn_post_Click(object sender, EventArgs e)
		{
			DecchiCore.Instance.Run();
		}

		#endregion

	}
}
