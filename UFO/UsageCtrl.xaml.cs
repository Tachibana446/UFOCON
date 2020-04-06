using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UFO
{
    /// <summary>
    /// UsageCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class UsageCtrl : UserControl
    {
        public UsageCtrl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("./readme.txt"))
            {
                textBlock.Text = File.ReadAllText("./readme.txt");
            }
            else
            {
                textBlock.Text =
    @"# UFOCON
UFO SAを操作するWPFソフト
有志作成のCSVファイルによる連動が可能。

# How to Use

# メインタブ
画面右上CSVグループの「開く」からCSVファイルを選択  
画面下部音声グループの「開く」から音声ファイルを選択  
再生ボタンで再生。CSVに記載された時刻・強弱でUFOを振動させる  
+10,-5ボタンはそれぞれ10秒進む・5秒戻る。音声グループ内のスライダーを動かすことでも再生位置の変更が可能。  


## CSVのフォーマット
先人たちのファイルに従い

時間（デシ秒）|回転方向(0or1)|強度(0~100%)
---|---|---
3000|1|40
3200|0|0
...|...|...

の形式にしている。
上記の例だと300秒から40%の速度で逆回転し、320秒の時点で止まる。

# シリアルポートタブ(UFOの設定)
付属のUSB無線親機をPCに接続する。起動時に刺さっていた場合は自動で認識する。  
起動後に挿した場合や認識されない場合は「シリアルポート」タブの「再設定」ボタンを押されたし。  
どうしても該当ポートが見つからなかった場合「一覧の更新」ボタンを押してからドロップダウンでUSBのポートを指定して「設定」ボタンを押す。  
どのポートがUFOのものかはWindowsデバイスマネージャーのポートの中に「Vorze～～」といった名前のポートがあるはず。（COM5など）  

ドライバが無いとCOMが認識されないかも。  
[UFOSAのドングルのドライバ（公式）](https://www.vorze.jp/en/support/)

# ランダムパネル
ランダムにUFOを動かして遊ぶモード  
パターンは 正回転・逆回転・徐々に強く・徐々に弱く・小刻みに動かす・一時停止 の6パターンをランダムに決定  
ランダムの強度などはスライダーで調整可能  

- 最小・最大時間：1回のパターンが実行される時間の最小・最大値
    - デフォルトでは1～5秒
- 最小・最大停止時間：一時停止パターンの時間
    - デフォルトでは1～3秒
- 最小・最大パワー：UFOのパワーをこの範囲の中でランダムに選ぶ
    - デフォルトでは0～80%

右側の再生ボタン（▶マーク）で開始・停止

# グラフタブ
CSVの内容を縦軸に強さ、横軸を時間としてグラフで見れる機能  
試験的につけてみたので使い勝手は微妙



";
            }
        }
    }
}
