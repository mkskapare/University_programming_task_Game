using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace rpggame
{
    public sealed partial class MainWindow : Window
    {
        private Hero hero;
        private readonly int step = 1;
        private DispatcherTimer movementTimer = new();
        private DispatcherTimer castleMovementTimer = new();
        private readonly MediaPlayer mediaPlayer = new();
        private readonly List<Star> stars = new();
        private readonly List<Slime> slimes = new();
        private readonly List<Potion> potions = new();
        private readonly List<Castle> castles = new();
        private readonly List<Crab> crabs = new();
        private readonly List<Beloved> beloveds = new();
        private readonly List<PalmTree> palmTrees = new();
        private Kraken kraken;
        private readonly List<Tentacle> Tentacles = new();
        private bool isMoving = false;
        private bool isCastleMoving = false;
        private bool isKrakenDefeated = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            isMoving = false;
            mediaPlayer = new MediaPlayer();
        }

        private void InitializeTimer()
        {
            movementTimer = new DispatcherTimer();
            movementTimer.Tick += new EventHandler(MovementTimer_Tick);
            movementTimer.Interval = TimeSpan.FromMilliseconds(100);

            castleMovementTimer = new DispatcherTimer();
            castleMovementTimer.Tick += new EventHandler(CastleMovementTimer_Tick);
            castleMovementTimer.Interval = TimeSpan.FromMilliseconds(100);
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            if (hero != null && PlayerImage != null && isMoving)
            {
                int currentRow = Grid.GetRow(PlayerImage);
                int currentColumn = Grid.GetColumn(PlayerImage);

                int newRow = currentRow;
                int newColumn = currentColumn;

                if (Keyboard.IsKeyDown(Key.Up))
                {
                    newRow = Math.Max(0, currentRow - step);
                    if (IsPalmTreeOccupied(newRow, currentColumn))
                    {
                        newRow = currentRow;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Down))
                {
                    newRow = Math.Min(grdMapCells.RowDefinitions.Count - 1, currentRow + step);
                    if (IsPalmTreeOccupied(newRow, currentColumn))
                    {
                        newRow = currentRow;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Left))
                {
                    newColumn = Math.Max(0, currentColumn - step);
                    if (IsPalmTreeOccupied(currentRow, newColumn))
                    {
                        newColumn = currentColumn;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Right))
                {
                    newColumn = Math.Min(grdMapCells.ColumnDefinitions.Count - 1, currentColumn + step);
                    if (IsPalmTreeOccupied(currentRow, newColumn))
                    {
                        newColumn = currentColumn;
                    }
                }

                if (!IsPalmTreeOccupied(newRow, newColumn))
                {
                    HandleInteraction(newRow, newColumn);

                    Grid.SetRow(PlayerImage, newRow);
                    Grid.SetColumn(PlayerImage, newColumn);
                    PlayerImage.Focus();
                }
            }
        }

        private void CastleMovementTimer_Tick(object sender, EventArgs e)
        {
            if (isCastleMoving && hero != null && CastlePlayerImage != null)
            {
                        int currentRow = Grid.GetRow(CastlePlayerImage);
                        int currentColumn = Grid.GetColumn(CastlePlayerImage);

                        int newRow = currentRow;
                        int newColumn = currentColumn;

                        if (Keyboard.IsKeyDown(Key.Up))
                        {
                            newRow = Math.Max(0, currentRow - step);
                        }
                        else if (Keyboard.IsKeyDown(Key.Down))
                        {
                            newRow = Math.Min(CastleMapCells.RowDefinitions.Count - 1, currentRow + step);
                        }
                        else if (Keyboard.IsKeyDown(Key.Left))
                        {
                            newColumn = Math.Max(0, currentColumn - step);
                        }
                        else if (Keyboard.IsKeyDown(Key.Right))
                        {
                            newColumn = Math.Min(CastleMapCells.ColumnDefinitions.Count - 1, currentColumn + step);
                        }

                        CastleHandleInteraction(newRow, newColumn);

                        Grid.SetRow(CastlePlayerImage, newRow);
                        Grid.SetColumn(CastlePlayerImage, newColumn);
                        CastlePlayerImage.Focus();
            }
        }

        private void CastleHandleInteraction(int newRow, int newColumn)
        {
            bool tentacleInteracted = false;

            foreach (var tentacle in Tentacles.ToList())
            {
                if (newRow == tentacle.Row && newColumn == tentacle.Column)
                {
                    tentacleInteracted = true;
                    AttackTentacle(tentacle);
                    break;
                }
            }

            if (isKrakenDefeated)
            {
                foreach (var beloved in beloveds.ToList())
                {
                    if (newRow == beloved.Row && newColumn == beloved.Column)
                    {
                        int playerRow = Grid.GetRow(CastlePlayerImage);
                        int playerColumn = Grid.GetColumn(CastlePlayerImage);

                        if (beloved.Row == playerRow && beloved.Column == playerColumn)
                        {
                            ShowFinishGameStoryPanel();
                            break;
                        }
                    }
                }
            }
        }

        private void ShowFinishGameStoryPanel()
        {
            FinishGameStory.Visibility = Visibility.Visible;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
            CastleStoryPanel.Visibility = Visibility.Collapsed;
            CastleContent.Visibility = Visibility.Collapsed;
        }

        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            FinishGameStory.Visibility = Visibility.Collapsed;
            leave.Visibility = Visibility.Visible;
        }
            private void HandleInteraction(int newRow, int newColumn)
        {
            bool starInteracted = false;
            bool slimeInteracted = false;
            bool potionInteracted = false;
            bool crabInteracted = false;
            bool castleInteracted = false;
            bool palmTreeOccupied = PositionOccupied(newRow, newColumn);

            foreach (var star in stars.ToList())
            {
                if (newRow == star.Row && newColumn == star.Column)
                {
                    grdMapCells.Children.Remove(star.StarImage);
                    ShowStoryPanel();
                    starInteracted = true;
                    stars.Remove(star);
                    break;
                }
            }

            foreach (var slime in slimes.ToList())
            {
                if (newRow == slime.Row && newColumn == slime.Column)
                {
                    grdMapCells.Children.Remove(slime.SlimeImage);
                    ShowBlueSlimeStoryPanel();
                    slimeInteracted = true;
                    slimes.Remove(slime);
                    break;
                }
            }

            foreach (var crab in crabs.ToList())
            {
                if (newRow == crab.Row && newColumn == crab.Column)
                {
                    grdMapCells.Children.Remove(crab.CrabImage);
                    ShowCrabStoryPanel();
                    crabInteracted = true;
                    crabs.Remove(crab);
                    break;
                }
            }

            foreach (var potion in potions.ToList())
            {
                if (newRow == potion.Row && newColumn == potion.Column)
                {
                    ShowPotionStoryWindow();
                    grdMapCells.Children.Remove(potion.PotionImage);
                    potions.Remove(potion);
                    potionInteracted = true;
                    break;
                }
            }

            bool crabDefeated = crabs.Count == 0;

            foreach (var castle in castles.ToList())
            {
                if (newRow == castle.Row && newColumn == castle.Column && crabDefeated)
                {
                    ShowCastleStoryPanel();
                    grdMapCells.Children.Remove(castle.CastleImage);
                    castles.Remove(castle);
                    castleInteracted = true;
                    break;
                }
            }

            if (!starInteracted && !slimeInteracted && !potionInteracted && !castleInteracted && !palmTreeOccupied)
            {
                if (crabDefeated)
                {
                    MovePlayerTo(newRow, newColumn);
                }
            }

        }
        private void ShowCastleStoryPanel()
        {
            GameContent.Visibility = Visibility.Collapsed;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
            CastleStoryPanel.Visibility = Visibility.Visible;
        }
        private void BeginCastleQuest_Click(object sender, RoutedEventArgs e)
        {
            CastleStoryPanel.Visibility = Visibility.Collapsed;
            grdMapCells.Visibility = Visibility.Collapsed;
            herostats.Visibility = Visibility.Visible;
            HeroNameDisplay.Visibility = Visibility.Visible;
            HeroHPDisplay.Visibility = Visibility.Visible;
            HeroStrengthDisplay.Visibility = Visibility.Visible;
            CastleContent.Visibility = Visibility.Visible;
            
            castleMovementTimer.Start();
            DisplayHeroStats();
            LoadCastlePlayerImage();
            GenerateBeloved();
            SpawnKraken();
            SpawnTentacles();

            string krakenMusicPath = @"F:/homework/gameprojectfinal/rpggame/rpggame/Media/fight.mp3";
            mediaPlayer.Open(new Uri(krakenMusicPath, UriKind.Relative));
            mediaPlayer.Play();
        }
        private void AttackTentacle(Tentacle tentacle)
        {
            tentacle.HP -= hero.Strength;

            if (tentacle.HP <= 0)
            {
                CastleMapCells.Children.Remove(tentacle.TentacleImage);
                Tentacles.Remove(tentacle);

                if (!isKrakenDefeated)
                {
                    ShowTentacleDefeatedStoryPanel();
                }

                kraken.HP -= 50;

                if (kraken.HP <= 0)
                {
                    isKrakenDefeated = true;
                    CastleMapCells.Children.Remove(kraken.KrakenImage);
                    kraken = null;

                    ShowKrakenDefeatedStoryPanel(); 

                    string musicPath = @"F:/homework/gameprojectfinal/rpggame/rpggame/Media/CastleMusic.mp3";
                    mediaPlayer.Open(new Uri(musicPath, UriKind.Relative));
                    mediaPlayer.MediaEnded += LoopMusic;
                    mediaPlayer.Play();
                }
            }
            else
            {
                hero.HP -= 5;
                if (hero.HP <= 0)
                {
                    DefeatedByKraken();
                }
            }
        }
        private void DefeatedByKraken()
        {
            DefeatbyKrakenStory.Visibility = Visibility.Visible;
            CastleContent.Visibility = Visibility.Collapsed;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
        }
        private void ShowKrakenDefeatedStoryPanel()
        {
            VictoryTentacleStory.Visibility = Visibility.Collapsed;
            VictoryKrakenStory.Visibility = Visibility.Visible;
            CastleContent.Visibility = Visibility.Collapsed;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
        }
        private void ShowTentacleDefeatedStoryPanel()
        {
            VictoryTentacleStory.Visibility = Visibility.Visible;
            CastleContent.Visibility = Visibility.Collapsed;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
        }
        private void ContinueKrakenStory_Click (object sender, RoutedEventArgs e)
        {
            VictoryKrakenStory.Visibility = Visibility.Collapsed;
            VictoryTentacleStory.Visibility = Visibility.Collapsed;
            CastleContent.Visibility = Visibility.Visible;
            herostats.Visibility = Visibility.Visible;
            HeroNameDisplay.Visibility = Visibility.Visible;
            HeroHPDisplay.Visibility = Visibility.Visible;
            HeroStrengthDisplay.Visibility = Visibility.Visible;
        }
        private void ContinueKrakenDefeatStory_Click (object sender, RoutedEventArgs e)
        {
            EndGame();
        }
        private void ShowPotionStoryWindow()
        {
            PotionStory.Visibility = Visibility.Visible;
            GameContent.Visibility = Visibility.Collapsed;
        }
        
        private void UsePotion_Click(object sender, RoutedEventArgs e)
        {
            hero.HP += 100;
            DisplayHeroStats();
            PotionUseStory.Visibility = Visibility.Visible;
            PotionStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Collapsed;
        }
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            PotionUseStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Visible;
        }
        private void MovePlayerTo(int newRow, int newColumn)
        {
            Grid.SetRow(PlayerImage, newRow);
            Grid.SetColumn(PlayerImage, newColumn);
            PlayerImage.Focus();
        }
        private void ShowCrabStoryPanel()
        {
            CrabStoryPanel.Visibility = Visibility.Visible;
            GameContent.Visibility = Visibility.Collapsed;
        }
        private void Fightcrab_Click(object sender, RoutedEventArgs e)
        {
            FightCrab();
        }
        private void FightCrab()
        {
            int crabHP = 200;
            int playerHP = hero.HP;
            int playerStrength = hero.Strength;

            while (crabHP > 0 && playerHP > 0)
            {
                crabHP -= playerStrength;

                if (crabHP <= 0)
                {
                    playerStrength += 10;
                    playerHP = 300;
                    hero.Strength = playerStrength;
                    hero.HP = playerHP;
                    VictoryCrabStory.Visibility = Visibility.Visible;
                    GameContent.Visibility = Visibility.Collapsed;
                    DisplayHeroStats();
                    CrabStoryPanel.Visibility = Visibility.Collapsed;

                    return;
                }

                playerHP -= 8;
                hero.HP = playerHP;
                if (playerHP <= 0)
                {
                    KingCrabLoss();
                    DisplayHeroStats();
                    return;
                }
            }
        }
        private void ContinueStory_Click(object sender, RoutedEventArgs e)
        {
            VictoryCrabStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Visible;
        }
        private void KingCrabLoss()
        {
            Killedbycrab.Visibility = Visibility.Visible;
            CrabStoryPanel.Visibility =Visibility.Collapsed;
        }
        private void CrabDefeat_Click(object sender, RoutedEventArgs e)
        {
            EndGame();
        }

        private void ShowBlueSlimeStoryPanel()
        {
            BlueslimeStoryPanel.Visibility = Visibility.Visible;
            GameContent.Visibility = Visibility.Collapsed;
        }
        private void Fightslime_Click(object sender, RoutedEventArgs e)
        {
            FightSlime();
        }
        private void FightSlime()
        {
            int slimeHP = 50;
            int playerHP = hero.HP;
            int playerStrength = 5;

            while (slimeHP > 0 && playerHP > 0)
            {
                slimeHP -= playerStrength;

                if (slimeHP <= 0)
                {
                    playerStrength += 5;
                    hero.Strength = playerStrength;
                    SlimeVictoryStory.Visibility = Visibility.Visible;
                    GameContent.Visibility = Visibility.Collapsed;
                    DisplayHeroStats();
                    BlueslimeStoryPanel.Visibility=Visibility.Collapsed;
                    
                    return;
                }

                playerHP -= 1;
                hero.HP = playerHP;
                if (playerHP <= 0)
                {
                    BlueSlimeLoss();
                    DisplayHeroStats();
                    return;
                }
            }
        }
        private void SlimeContinueStory_Click(object sender, RoutedEventArgs e)
        {
            SlimeVictoryStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Visible;
        }
        private void BlueSlimeLoss()
        {
            Killedbyslime.Visibility = Visibility.Visible;
            BlueslimeStoryPanel.Visibility = Visibility.Collapsed;
        }
        private void SlimeDefeat_Click(object sender, RoutedEventArgs e)
        {
            EndGame();
        }
        private void EndGame()
        {
            Defeat.Visibility = Visibility.Visible;
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Collapsed;

            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            Process.Start(appPath);

            Application.Current.Shutdown();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            StartGame_Click(sender, e);
        }
        private void Escapecrab_Click(object sender, RoutedEventArgs e)
        {
            if (hero != null)
            {
                int playerHP = hero.HP;

                playerHP -= 100;

                if (playerHP <= 0)
                {
                    KingCrabLoss();
                    DisplayHeroStats();
                }
                else
                {
                    hero.HP = playerHP;
                    DisplayHeroStats();
                    CrabEscapeStory.Visibility = Visibility.Visible;
                    GameContent.Visibility = Visibility.Collapsed;
                    CrabStoryPanel.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void CrabEscapeContinueStory_Click(object sender, RoutedEventArgs e)
        {
            CrabEscapeStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Visible;
        }
        private void Escapeslime_Click(object sender, RoutedEventArgs e)
        {
            if (hero != null)
            {
                int playerHP = hero.HP;

                playerHP -= 5;

                if (playerHP <= 0)
                {
                    BlueSlimeLoss();
                    DisplayHeroStats();
                }
                else
                {
                    hero.HP = playerHP;
                    DisplayHeroStats();
                    SlimeEscapeStory.Visibility = Visibility.Visible;
                    GameContent.Visibility = Visibility.Collapsed;
                    BlueslimeStoryPanel.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void SlimeEscapeContinueStory_Click(object sender, RoutedEventArgs e)
        {
            SlimeEscapeStory.Visibility = Visibility.Collapsed;
            GameContent.Visibility = Visibility.Visible;
        }
        private void ShowStoryPanel()
        {
            herostats.Visibility = Visibility.Collapsed;
            HeroNameDisplay.Visibility = Visibility.Collapsed;
            HeroHPDisplay.Visibility = Visibility.Collapsed;
            HeroStrengthDisplay.Visibility = Visibility.Collapsed;
            StoryPanel.Visibility = Visibility.Visible;
            GameContent.Visibility = Visibility.Collapsed;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            CastleContent.Visibility = Visibility.Collapsed;
            InitialContent.Visibility = Visibility.Collapsed;
            StoryIntroScreen.Visibility = Visibility.Collapsed;
            HeroNameInput.Visibility = Visibility.Visible;
            GameGrid.Visibility = Visibility.Collapsed;
        }

        private void SaveName_Click(object sender, RoutedEventArgs e)
        {
            string heroName = HeroNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(heroName))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }
            hero = new Hero
            {
                Name = heroName,
                HP = 100,
                Strength = 5,
            };

            ShowWelcomePanel(heroName);
            InitialContent.Visibility = Visibility.Collapsed;
            HeroNameInput.Visibility = Visibility.Collapsed;
            StoryIntroScreen.Visibility = Visibility.Collapsed;

        }
        private void ShowWelcomePanel(string playerName)
        {
            TextBlock welcomeTextBlock = WelcomePanel.Children[0] as TextBlock;
            welcomeTextBlock.Text = $"Welcome, {playerName}! Let's start the game!";
            WelcomePanel.Visibility = Visibility.Visible;
        }
        private void StartGamePlayer_Click(object sender, RoutedEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            StoryIntroScreen.Visibility = Visibility.Visible;
        }
        private void FindBeloved_Click(object sender, RoutedEventArgs e)
        {
            herostats.Visibility = Visibility.Visible;
            HeroNameDisplay.Visibility = Visibility.Visible;
            HeroHPDisplay.Visibility = Visibility.Visible;
            HeroStrengthDisplay.Visibility = Visibility.Visible;
            GameContent.Visibility = Visibility.Visible;
            StoryPanel.Visibility = Visibility.Collapsed;
        }
        private void BeginQuest_Click(object sender, RoutedEventArgs e)
        {
            StoryIntroScreen.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            DisplayHeroStats();
            LoadPlayerImage();
            GenerateStar();
            GenerateBlueSlime();
            GenerateCastle();
            GeneratePotion();
            GenerateCrab();
            int numberOfPalmTrees = 8;
            GeneratePalmTrees(numberOfPalmTrees);

            string musicPath = @"F:/homework/gameprojectfinal/rpggame/rpggame/Media/explorationsong.mp3";
            mediaPlayer.Open(new Uri(musicPath, UriKind.Relative));
            mediaPlayer.MediaEnded += LoopMusic;
            mediaPlayer.Play();
        }
        
        private void LoopMusic(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }
        private void DisplayHeroStats()
        {
            if (hero != null)
            {
                HeroNameDisplay.Text = $"Name: {hero.Name}";
                HeroHPDisplay.Text = $"HP: {hero.HP}";
                HeroStrengthDisplay.Text = $"Strength: {hero.Strength}";
            }
        }
        private void LoadPlayerImage()
        {
                string imagePath = @"F:/homework/gameprojectfinal/rpggame/rpggame/Media/player.png";
                BitmapImage playerImage = new BitmapImage(new Uri(imagePath)); 
                PlayerImage.Source = playerImage;
                PlayerImage.Visibility = Visibility.Visible;
                Grid.SetColumn(PlayerImage, 5);
                Grid.SetRow(PlayerImage, 5);
        }
        private void LoadCastlePlayerImage()
        {
            string imagePath = @"F:/homework/gameprojectfinal/rpggame/rpggame/Media/player.png";
            BitmapImage playerImage = new BitmapImage(new Uri(imagePath));
            CastlePlayerImage.Source = playerImage;
            CastlePlayerImage.Visibility = Visibility.Visible;
            Grid.SetColumn(CastlePlayerImage, 2);
            Grid.SetRow(CastlePlayerImage, 4);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (GameContent.Visibility == Visibility.Visible)
            {
                isMoving = true;
                movementTimer.Start();
            }
            else if (CastleContent.Visibility == Visibility.Visible)
            {
                isCastleMoving = true;
                castleMovementTimer.Start();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            isMoving = false;
            movementTimer.Stop();

            isCastleMoving = false;
            castleMovementTimer.Stop();
        }
        private void GenerateStar()
        {
                Image starImage = new Image();
                BitmapImage starBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/star.png"));
                starImage.Source = starBitmap;

                starImage.Width = 50;
                starImage.Height = 50;

                Random random = new Random();
                int row, column;

            do
            {
                row = random.Next(0, grdMapCells.RowDefinitions.Count);
                column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
            } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

            Grid.SetRow(starImage, row);
                Grid.SetColumn(starImage, column);

                stars.Add(new Star { StarImage = starImage, Row = row, Column = column });

                grdMapCells.Children.Add(starImage);
        }
        private void GenerateBlueSlime()
        {
                Image slimeImage = new Image();
                BitmapImage slimeBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/blueslime.gif"));
                slimeImage.Source = slimeBitmap;

                slimeImage.Width = 50;
                slimeImage.Height = 50;

                Random random = new Random();
                int row, column;

            do
            {
                row = random.Next(0, grdMapCells.RowDefinitions.Count);
                column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
            } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

            Grid.SetRow(slimeImage, row);
                Grid.SetColumn(slimeImage, column);

                slimes.Add(new Slime { SlimeImage = slimeImage, Row = row, Column = column });

                grdMapCells.Children.Add(slimeImage);
        }
        private void GenerateCrab()
        {
            Image crabImage = new Image();
            BitmapImage crabBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/crab.png"));
            crabImage.Source = crabBitmap;

            crabImage.Width = 50;
            crabImage.Height = 50;

            Random random = new Random();
            int row, column;

            do
            {
                row = random.Next(0, grdMapCells.RowDefinitions.Count);
                column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
            } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

            Grid.SetRow(crabImage, row);
            Grid.SetColumn(crabImage, column);

            crabs.Add(new Crab { CrabImage = crabImage, Row = row, Column = column });

            grdMapCells.Children.Add(crabImage);
        }
        private void GenerateCastle()
        {
                Image castleImage = new Image();
                BitmapImage castleBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/castle.png"));
                castleImage.Source = castleBitmap;

                castleImage.Width = 50;
                castleImage.Height = 50;

                Random random = new Random();
                int row, column;

            do
            {
                row = random.Next(0, grdMapCells.RowDefinitions.Count);
                column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
            } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

            Grid.SetRow(castleImage, row);
                Grid.SetColumn(castleImage, column);

                castles.Add(new Castle { CastleImage = castleImage, Row = row, Column = column });

                grdMapCells.Children.Add(castleImage);
        }
        private void GeneratePotion()
        {
            Image potionImage = new Image();
            BitmapImage potionBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/Potion.png"));
            potionImage.Source = potionBitmap;

            potionImage.Width = 25;
            potionImage.Height = 25;

            Random random = new Random();
            int row, column;

            do
            {
                row = random.Next(0, grdMapCells.RowDefinitions.Count);
                column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
            } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

            Grid.SetRow(potionImage, row);
            Grid.SetColumn(potionImage, column);

            Potion potion = new Potion { PotionImage = potionImage, Row = row, Column = column };
            potions.Add(potion);

            grdMapCells.Children.Add(potionImage);
        }
        private void GeneratePalmTrees(int numberOfTrees)
        {
            Random random = new Random();

            for (int i = 0; i < numberOfTrees; i++)
            {
                Image palmImage = new Image();
                BitmapImage palmBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/palm.png"));
                palmImage.Source = palmBitmap;

                palmImage.Width = 50;
                palmImage.Height = 50;

                int row, column;

                do
                {
                    row = random.Next(0, grdMapCells.RowDefinitions.Count);
                    column = random.Next(0, grdMapCells.ColumnDefinitions.Count);
                } while (PositionOccupied(row, column) || (row == Grid.GetRow(PlayerImage) && column == Grid.GetColumn(PlayerImage)));

                Grid.SetRow(palmImage, row);
                Grid.SetColumn(palmImage, column);

                grdMapCells.Children.Add(palmImage);
            }
        }

        private bool IsPalmTreeOccupied(int row, int column)
        {
            foreach (var tree in palmTrees)
            {
                if (tree.Row == row && tree.Column == column)
                {
                    return true;
                }
            }
            return false;
        }

        private bool PositionOccupied(int row, int column)
        {
            foreach (var star in stars)
            {
                if (star.Row == row && star.Column == column)
                    return true;
            }

            foreach (var slime in slimes)
            {
                if (slime.Row == row && slime.Column == column)
                    return true;
            }

            foreach (var crab in crabs)
            {
                if (crab.Row == row && crab.Column == column)
                    return true; 
            }

            foreach (var castle in castles)
            {
                if (castle.Row == row && castle.Column == column)
                    return true;
            }

            foreach (var potion in potions)
            {
                if (potion.Row == row && potion.Column == column)
                    return true;
            }

            foreach (var palmTree in palmTrees)
            {
                if (palmTree.Row == row && palmTree.Column == column)
                    return true;
            }

            return false;
        }
        private void GenerateBeloved()
        {
            Image belovedImage = new Image();
            BitmapImage belovedBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/Beloved.png"));
            belovedImage.Source = belovedBitmap;

            belovedImage.Width = 100;
            belovedImage.Height = 100;

            Random random = new Random();
            int row, column;

            row = 1;
            column = 2;

            Grid.SetRow(belovedImage, row);
            Grid.SetColumn(belovedImage, column);

            Beloved beloved = new Beloved { BelovedImage = belovedImage, Row = row, Column = column };
            beloveds.Add(beloved);

            CastleMapCells.Children.Add(belovedImage);
        }
    
    private void SpawnKraken()
    {
        Image krakenImage = new Image();
        BitmapImage krakenBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/Kraken.png"));
        krakenImage.Source = krakenBitmap;

        krakenImage.Width = 100;
        krakenImage.Height = 100;

        int row = 2;
        int column = 2;

        Grid.SetRow(krakenImage, row);
        Grid.SetColumn(krakenImage, column);

        kraken = new Kraken { KrakenImage = krakenImage, Row = row, Column = column };

        CastleMapCells.Children.Add(krakenImage);
    }

        private void SpawnTentacles()
        {
            int[][] tentaclePositions = new int[][]
            {
            new int[] { 0, 0 }, new int[] { 0, 4 }, new int[] { 4, 0 }, new int[] { 4, 4 },
            new int[] { 2, 0 }, new int[] { 2, 4 },
            };

            foreach (var position in tentaclePositions)
            {
                Image tentacleImage = new Image();
                BitmapImage tentacleBitmap = new BitmapImage(new Uri(@"F:/homework/gameprojectfinal/rpggame/rpggame/Media/Tentacle.png"));
                tentacleImage.Source = tentacleBitmap;

                tentacleImage.Width = 50;
                tentacleImage.Height = 50;

                int row = position[0];
                int column = position[1];

                Grid.SetRow(tentacleImage, row);
                Grid.SetColumn(tentacleImage, column);

                Tentacle tentacle = new Tentacle { TentacleImage = tentacleImage, Row = row, Column = column, HP = 50, Strength = 5 };

                Tentacles.Add(tentacle);
                CastleMapCells.Children.Add(tentacleImage);
            }
        }
    }
    public class Hero
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int Strength { get; set; }
        public Hero()
        {
            HP = 100;
            Strength = 5;
        }
    }
    public class Star
    {
        public Image StarImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class Castle
    {
        public Image CastleImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class Potion
    {
        public Image PotionImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class PalmTree
    {
        public Image PalmTreeImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class Beloved
    {
        public Image BelovedImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
    public class Slime
    {
        public Image SlimeImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int HP { get; set; }
        public int Strength { get; set; }
    }
    public class Kraken
    {
        public Image KrakenImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int HP { get; set; }
        public int Strength { get; set; }
        public Kraken()
        {
            HP = 300;
            Strength = 10;
        }
    }
    public class Tentacle
    {
        public Image TentacleImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int HP { get; set; }
        public int Strength { get; set; }
    }
    public class Crab
    {
        public Image CrabImage { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int HP { get; set; }
        public int Strength { get; set; }
    }
}