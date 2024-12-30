    using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UnoOnline
{
    public partial class Form1 : Form
    {
        //private Label currentPlayerLabel;
        //private Label currentCardLabel;
        //private Panel PlayerHandPanel;
        //private Button skipTurnButton;
        //private Button drawCardButton;
        private Panel chatPanel;
        private TextBox chatInput;
        private RichTextBox chatHistory;
        private Timer timer;
        private Button yellUNOButton;
        private TableLayoutPanel mainLayout;
        private Panel gameStatusPanel;
        private FlowLayoutPanel playerCardsPanel;
        private Panel actionPanel;
        Card currentCard = GameManager.Instance.CurrentCard;

        private Button redButton;
        private Button greenButton;
        private Button yellowButton;
        private Button blueButton;

        // Update the image displayed in the PictureBox
        public void UpdateCurrentCardDisplay(Card currentCard)
        {
            string cardImagePath = "";
            if (currentCard.CardName.Contains("Wild"))
            {
                if (currentCard.Value == "Draw")
                    cardImagePath = Path.Combine("Resources", "CardImages", "Wild_Draw.png");
                else
                    cardImagePath = Path.Combine("Resources", "CardImages", "Wild.png");
            }
            else
            {
                // Construct the file path for the card image
                cardImagePath = Path.Combine("Resources", "CardImages", $"{currentCard.Color}_{currentCard.Value}.png");
            }

            if (File.Exists(cardImagePath))
            {
                // Load the image into the PictureBox
                currentCardPictureBox.Image = Image.FromFile(cardImagePath);
            }
        }


        // Thêm method hiển thị chat
        public void AddChatMessage(string sender, string message)
        {
            if (chatHistory.InvokeRequired)
            {
                BeginInvoke(new Action(() => AddChatMessage(sender, message)));
                return;
            }
            string formattedMessage = $"[{sender}]: {message}\n";
            chatHistory.AppendText(formattedMessage);
            chatHistory.ScrollToCaret();
        }


        public class CustomCard : UserControl
        {
            public CustomCard()
            {
                this.BackColor = Color.White; // Màu nền của thẻ
                this.BorderStyle = BorderStyle.FixedSingle; // Đường viền cho thẻ
            }
        }

        private Button CreateStyledButton(string text, int index)
        {
            return new Button
            {
                Text = text,
                Width = 130,
                Height = 40,
                Location = new Point(10, 10 + (index * 50)),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
        }


        private void ApplyCustomTheme()
        {
            // Set màu nền gradient
            this.Paint += (sender, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(41, 128, 185), // Màu xanh đậm
                    Color.FromArgb(44, 62, 80),   // Màu xám đen
                    90F))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            // Style cho buttons
            foreach (Control control in this.Controls)
            {
                if (control is Button btn)
                {
                    // Skip color buttons
                    if (btn.BackColor == Color.Red || btn.BackColor == Color.Green || btn.BackColor == Color.Yellow || btn.BackColor == Color.Blue)
                        continue;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.ForeColor = Color.White;
                    btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    btn.Cursor = Cursors.Hand;
                    btn.BackColor = Color.FromArgb(52, 152, 219);

                    // Hover effect
                    btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(41, 128, 185);
                    btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(52, 152, 219);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeGameBoard();
            ApplyCustomTheme();
            InitializeCustomComponents();
            this.ClientSize = new System.Drawing.Size(945, 540); // Kích thước nội dung (không bao gồm thanh tiêu đề và viền)
            this.StartPosition = FormStartPosition.CenterScreen; // Hiển thị Form ở giữa màn hình

            this.BackgroundImageLayout = ImageLayout.None; // Đảm bảo kích thước chính xác của hình ảnh được giữ nguyên
            if (this.BackgroundImage != null)
            {
                this.ClientSize = this.BackgroundImage.Size; // Đặt kích thước Form khớp với kích thước hình ảnh
            }
            // Tùy chọn: Ngăn thay đổi kích thước Form
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen; // Hiển thị Form ở giữa màn hình
        }

        public static void UpdateCurrentPlayerLabel(string playerName)
        {
            Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (form != null)
            {
                form.Invoke(new Action(() => form.currentPlayerLabel.Text = $"Lượt của: {playerName}"));
            }
        }

        private void InitializeGameBoard()
        {


            clientInfoLabel = new Label
            {
                Size = new Size(200, 30),
                Text = $"Tên: {Program.player.Name}",
                Font = new Font("Arial", 14),
                BackColor = Color.Transparent,
                Location = new Point(10, 10) // Góc trên bên trái
            };
            Controls.Add(clientInfoLabel);

            currentColor = new Label
            {
                Size = new Size(200, 30),
                Text = $"Màu hiện tại: {GameManager.Instance.CurrentCard.Color}",
                Font = new Font("Arial", 14),
                BackColor = Color.Transparent,
                Location = new Point(10, 40)
            };
            Controls.Add(currentColor);

            // PictureBox for current card
            currentCardPictureBox = new PictureBox
            {
                Size = new Size(this.ClientSize.Width / 6, this.ClientSize.Height / 3),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Location = new Point((this.ClientSize.Width - this.ClientSize.Width / 6) / 2, (this.ClientSize.Height - this.ClientSize.Height / 3) / 2 - 35),
                BorderStyle = BorderStyle.FixedSingle,
            };
            Controls.Add(currentCardPictureBox);

            // Label for current player
            currentPlayerLabel = new Label
            {
                Size = new Size(200, 30),
                Text = $"Lượt của: {GameManager.Instance.Players[0].Name}",
                Font = new Font("Arial", 14),
                BackColor = Color.Transparent,
                Location = new Point(currentCardPictureBox.Left + (currentCardPictureBox.Width - 200) / 2, currentCardPictureBox.Top - 40)
            };
            Controls.Add(currentPlayerLabel);

            // Panel for player hand
            PlayerHandPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
            };
            Controls.Add(PlayerHandPanel);

            // Draw card button
            drawCardButton = new Button
            {
                Size = new Size(100, 40),
                Text = "Rút bài",
                Location = new Point(this.ClientSize.Width - 120, (this.ClientSize.Height - 200) / 2 + 50),
                BackColor = Color.Empty
            };
            drawCardButton.Click += DrawCardButton_Click;
            Controls.Add(drawCardButton);

            yellUNOButton = new Button
            {
                Size = new Size(100, 40),
                Text = "UNO!",
                Location = new Point(this.ClientSize.Width - 120, (this.ClientSize.Height - 200) / 2 + 100)
            };
            yellUNOButton.Click += yellUNOButton_Click;
            Controls.Add(yellUNOButton);

            // Initialize color buttons
            redButton = new Button
            {
                Size = new Size(50, 50),
                BackColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(currentCardPictureBox.Right + 10, currentCardPictureBox.Bottom - 110)
            };
            Controls.Add(redButton);
            redButton.Click += RedButton_Click;

            greenButton = new Button
            {
                Size = new Size(50, 50),
                BackColor = Color.Green,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(currentCardPictureBox.Right + 70, currentCardPictureBox.Bottom - 110)
            };
            Controls.Add(greenButton);
            greenButton.Click += GreenButton_Click;

            yellowButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                BackColor = Color.Yellow,
                Location = new Point(currentCardPictureBox.Right + 10, currentCardPictureBox.Bottom - 50)
            };
            Controls.Add(yellowButton);
            yellowButton.Click += YellowButton_Click;

            blueButton = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                BackColor = Color.Blue,
                Location = new Point(currentCardPictureBox.Right + 70, currentCardPictureBox.Bottom - 50)
            };
            Controls.Add(blueButton);
            blueButton.Click += BlueButton_Click;
            // Initialize deck images
            InitializeDeckImages();

            // Initialize chat panel
            InitializeChatPanel();
        }

        private void yellUNOButton_Click(object sender, EventArgs e)
        {
            Message yellUNOMessage = new Message(MessageType.YellUNO, new List<string> { Program.player.Name });
            ClientSocket.SendData(yellUNOMessage);
            // Disable uno button
            yellUNOButton.Enabled = false;
        }


        public void DisplayPlayerHand(List<Card> playerHand)
        {
            if (playerHand == null)
            {
                System.Diagnostics.Debug.WriteLine("Player hand cannot be null.");
                throw new ArgumentNullException(nameof(playerHand), "Player hand cannot be null.");
            }
            PlayerHandPanel.Controls.Clear(); // Clear existing controls
            int xOffset = 10;
            int yOffset = 10;
            int cardWidth = 100; // Increased card width
            int cardHeight = 150; // Increased card height

            foreach (var card in playerHand)
            {
                if (card == null)
                {
                    System.Diagnostics.Debug.WriteLine("Found a null card in player hand.");
                    throw new ArgumentException("Card cannot be null", nameof(card));
                }

                System.Diagnostics.Debug.WriteLine("Displaying card: " + card.CardName);

                Button cardButton = new Button
                {
                    Size = new Size(cardWidth, cardHeight),
                    Location = new Point(xOffset, yOffset),
                    BackgroundImage = GetCardImage(card),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    FlatStyle = FlatStyle.Flat,
                    Tag = card,
                    BackColor = Color.White,
                    FlatAppearance = { BorderSize = 1, BorderColor = Color.Black }
                };

                cardButton.FlatAppearance.MouseOverBackColor = Color.LightGray;
                cardButton.FlatAppearance.BorderSize = 1;

                cardButton.Click += CardButton_Click;

                PlayerHandPanel.Controls.Add(cardButton);

                xOffset += cardWidth + 5; // Space between cards
            }
        }
        private Image GetCardImage(Card card)
        {
            if (card == null)
            {
                MessageBox.Show("Card is null in GetCardImage.");
                return null;
            }

            string cardImagePath = "";

            // Xử lý các thẻ đặc biệt như "Wild"
            if (card.Color == "Wild")
            {
                if (card.Value == "Draw")
                    cardImagePath = Path.Combine("Resources", "CardImages", "Wild_Draw.png");
                else
                    cardImagePath = Path.Combine("Resources", "CardImages", "Wild.png");
            }
            else
            {
                // Đối với các lá bài màu
                cardImagePath = Path.Combine("Resources", "CardImages", $"{card.Color}_{card.Value}.png");
            }

            if (File.Exists(cardImagePath))
            {
                return Image.FromFile(cardImagePath);
            }
            else
            {
                MessageBox.Show($"Card image not found: {cardImagePath}");
                return null;
            }
        }
        private void EnableColorButtons()
        {
            redButton.Enabled = true;
            greenButton.Enabled = true;
            yellowButton.Enabled = true;
            blueButton.Enabled = true;
        }
        public void DisableColorButtons()
        {
            redButton.Enabled = false;
            greenButton.Enabled = false;
            yellowButton.Enabled = false;
            blueButton.Enabled = false;
        }
        
        private void CardButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            Card selectedCard = clickedButton.Tag as Card;

            if (GameManager.Instance.IsValidMove(selectedCard))
            {
                if (selectedCard.Color == "Wild")
                {
                    EnableColorButtons();
                }
                else
                {
                    ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { GameManager.Instance.Players[0].Name, (GameManager.Instance.Players[0].Hand.Count - 1).ToString(), selectedCard.CardName, selectedCard.Color }));
                }
                GameManager.Instance.CurrentCard = selectedCard;
                GameManager.Instance.Players[0].Hand.Remove(selectedCard);
                UpdateCurrentCardDisplay(selectedCard);
                PlayerHandPanel.Controls.Remove(clickedButton);
                DisableCardAndDrawButton();
            }
            else
            {
                MessageBox.Show("Invalid move.");
            }
        }
        private void RedButton_Click(object sender, EventArgs e)
        {
            GameManager.Instance.CurrentCard.Color = "Red";
            //Gửi thông điệp đến server theo định dạng DanhBai;ID;SoLuongBaiTrenTay;CardName;color
            ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { GameManager.Instance.Players[0].Name, (GameManager.Instance.Players[0].Hand.Count).ToString(), GameManager.Instance.CurrentCard.CardName, GameManager.Instance.CurrentCard.Color }));
            DisableColorButtons();
            EnableCardAndDrawButton();
        }
        private void GreenButton_Click(object sender, EventArgs e)
        {
            GameManager.Instance.CurrentCard.Color = "Green";
            ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { GameManager.Instance.Players[0].Name, (GameManager.Instance.Players[0].Hand.Count).ToString(), GameManager.Instance.CurrentCard.CardName, GameManager.Instance.CurrentCard.Color }));
            DisableColorButtons();
            DisableCardAndDrawButton();
        }
        private void YellowButton_Click(object sender, EventArgs e)
        {
            GameManager.Instance.CurrentCard.Color = "Yellow";
            ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { GameManager.Instance.Players[0].Name, (GameManager.Instance.Players[0].Hand.Count).ToString(), GameManager.Instance.CurrentCard.CardName, GameManager.Instance.CurrentCard.Color }));
            DisableColorButtons();
            DisableCardAndDrawButton();
        }
        private void BlueButton_Click(object sender, EventArgs e)
        {
            GameManager.Instance.CurrentCard.Color = "Blue";
            ClientSocket.SendData(new Message(MessageType.DanhBai, new List<string> { GameManager.Instance.Players[0].Name, (GameManager.Instance.Players[0].Hand.Count).ToString(), GameManager.Instance.CurrentCard.CardName, GameManager.Instance.CurrentCard.Color }));
            DisableColorButtons();
            DisableCardAndDrawButton();
        }
        private void DrawCardButton_Click(object sender, EventArgs e)
        {
            ClientSocket.SendData(new Message(MessageType.RutBai, new List<string> { Program.player.Name, (GameManager.Instance.Players[0].Hand.Count + 1).ToString() }));
            // Update clientInfoLabel
            clientInfoLabel.Text = $"Tên: {Program.player.Name}";
            DisableColorButtons();
            DisableCardAndDrawButton();
        }

        public void EnableCardAndDrawButton()
        {
            // Enable the Draw Card button
            drawCardButton.Enabled = true;
            // Enable the Card buttons
            foreach (Button cardButton in PlayerHandPanel.Controls)
            {
                cardButton.Enabled = true;
            }
        }
        public void DisableCardAndDrawButton()
        {
            // Disable the Draw Card button
            drawCardButton.Enabled = false;
            // Disable the Card buttons
            foreach (Button cardButton in PlayerHandPanel.Controls)
            {
                cardButton.Enabled = false;
            }
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(945, 540);
            this.Name = "Form1";
            this.ResumeLayout(false);
        }

        private Button drawCardButton;
        private FlowLayoutPanel PlayerHandPanel;

        private PictureBox currentCardPictureBox;
        private Label currentColor;
        private Label currentPlayerLabel;
        private Label clientInfoLabel; // Nhãn để hiển thị tên và số bài của client

        private async void AnimateCardDrawing(Card card)
        {
            Button cardButton = new Button
            {
                Size = new Size(80, 120),
                BackgroundImage = GameResources.GetCardImage(card),
                BackgroundImageLayout = ImageLayout.Stretch,
                FlatStyle = FlatStyle.Flat,
                Tag = card,
                BackColor = Color.White,
                FlatAppearance = { BorderSize = 1, BorderColor = Color.Black }
            };

            cardButton.FlatAppearance.MouseOverBackColor = Color.LightGray;
            cardButton.FlatAppearance.BorderSize = 1;

            Controls.Add(cardButton);

            Point startPoint = new Point(500, 110); // Starting point (deck location)
            Point endPoint = new Point(20 + (GameManager.Instance.Players[0].Hand.Count * 85), 60); // Ending point (player hand location)

            for (int i = 0; i <= 100; i += 5)
            {
                cardButton.Location = new Point(
                    startPoint.X + (endPoint.X - startPoint.X) * i / 100,
                    startPoint.Y + (endPoint.Y - startPoint.Y) * i / 100
                );
                await Task.Delay(10);
            }

            Controls.Remove(cardButton);
            DisplayPlayerHand(GameManager.Instance.Players[0].Hand);
        }

        private async void AnimateCardPlaying(Button cardButton, Card card)
        {
            Point startPoint = cardButton.Location; // Starting point (player hand location)
            Point endPoint = new Point(300, 50); // Ending point (center of the game board)

            for (int i = 0; i <= 100; i += 5)
            {
                cardButton.Location = new Point(
                    startPoint.X + (endPoint.X - startPoint.X) * i / 100,
                    startPoint.Y + (endPoint.Y - startPoint.Y) * i / 100
                );
                await Task.Delay(10);
            }

            PlayerHandPanel.Controls.Remove(cardButton);
            currentCard = card;
        }
        public static void YellUNOEnable()
        {
            // Assuming you have a reference to the Form1 instance
            Form1 form = Application.OpenForms.OfType<Form1>().FirstOrDefault();
            if (form != null)
            {
                form.Invoke(new Action(() => form.yellUNOButton.Enabled = true));
            }
        }

        private void InitializeCustomComponents()
        {
            // Initialize yellUNOButton
            yellUNOButton = new Button();
            yellUNOButton.Text = "Yell UNO!";
            yellUNOButton.Click += yellUNOButton_Click;
            // Add yellUNOButton to the form or a specific panel
            // Initialize other custom components if needed
        }

        public class CustomCardPanel : Panel
        {
            public CustomCardPanel()
            {
                this.AutoScroll = true; // Bật tính năng cuộn
            }
        }
        public void InitializeDeckImages()
        {
            var existingDeckImages = Controls.OfType<PictureBox>().Where(pb => pb.Tag != null && pb.Tag.ToString() == "DeckImage").ToList();
            var existingDeckLabels = Controls.OfType<Label>().Where(lbl => lbl.Tag != null && lbl.Tag.ToString() == "DeckLabel").ToList();
            foreach (var deckImageControl in existingDeckImages)
            {
                Controls.Remove(deckImageControl);
            }
            foreach (var deckLabelControl in existingDeckLabels)
            {
                Controls.Remove(deckLabelControl);
            }

            Image deckImage = Image.FromFile(@"Resources\CardImages\Deck.png");

            for (int i = 1; i < GameManager.Instance.Players.Count; i++) // Start from 1 to skip the current player
            {
                var player = GameManager.Instance.Players[i];
                PictureBox deckPictureBox = new PictureBox
                {
                    Size = new Size(100, 150),
                    Image = deckImage,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(this.ClientSize.Width - (120 + (i - 1) * 110), 20),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = "DeckImage"
                };

                Label deckLabel = new Label
                {
                    Size = new Size(100, 20),
                    Text = $"{player.Name}: {player.HandCount} cards",
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(deckPictureBox.Left, deckPictureBox.Bottom + 5),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.Transparent,
                    Tag = "DeckLabel"
                };

                Controls.Add(deckPictureBox);
                Controls.Add(deckLabel);
            }
            currentColor.Text = $"Màu hiện tại: {GameManager.Instance.CurrentCard.Color}";
        }

        private void InitializeChatPanel()
        {
            // Panel for chat
            chatPanel = new Panel
            {
                Size = new Size(250, currentCardPictureBox.Height), // Set the size of the chat panel
                Location = new Point(20, currentCardPictureBox.Top), // Align with currentCardPictureBox
                BackColor = Color.LightGray // Optional: Set the background color
            };

            // RichTextBox for chat history
            chatHistory = new RichTextBox
            {
                Dock = DockStyle.Top,
                Height = chatPanel.Height - 60, // Leave space for the input TextBox and Button
                ReadOnly = true,
                BackColor = Color.White
            };

            // TextBox for chat input
            chatInput = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 30
            };
            // Button to send chat message
            Button sendButton = new Button
            {
                Text = "Gửi",
                Dock = DockStyle.Bottom,
                Height = 30
            };
            sendButton.Click += SendButton_Click;

            // Add controls to chat panel
            chatPanel.Controls.Add(chatHistory);
            chatPanel.Controls.Add(chatInput);
            chatPanel.Controls.Add(sendButton);

            // Add chat panel to form
            Controls.Add(chatPanel);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = chatInput.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                ClientSocket.SendData(new Message(MessageType.Chat, new List<string> { Program.player.Name, message }));
                AddChatMessage("You", message);
                chatInput.Clear();
            }
        }

        // end aaasddd
    }
}