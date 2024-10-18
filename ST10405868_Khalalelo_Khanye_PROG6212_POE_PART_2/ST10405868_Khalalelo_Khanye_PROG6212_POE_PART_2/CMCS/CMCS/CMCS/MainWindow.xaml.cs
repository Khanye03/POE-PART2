using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CMCS
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Claim> _claims;
        public ObservableCollection<Claim> Claims
        {
            get { return _claims; }
            set
            {
                _claims = value;
                OnPropertyChanged("Claims");
            }
        }

        // Temporary storage for uploaded file path before claim submission
        private string _uploadedFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            Claims = new ObservableCollection<Claim>();
            this.DataContext = this;

            // Ensure the Uploads directory exists
            string uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedDocuments");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }
        }

        private void UploadDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Supporting Document",
                Filter = "PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx|Excel Files (*.xlsx)|*.xlsx",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                // Check file size (limit to 5 MB)
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("File size exceeds the 5 MB limit.", "File Size Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Check file extension
                string[] allowedExtensions = { ".pdf", ".docx", ".xlsx" };
                if (Array.IndexOf(allowedExtensions, fileInfo.Extension.ToLower()) < 0)
                {
                    MessageBox.Show("Invalid file type. Only PDF, DOCX, and XLSX files are allowed.", "File Type Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Generate a unique file name to prevent conflicts
                string uniqueFileName = $"{Guid.NewGuid()}{fileInfo.Extension}";
                string uploadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedDocuments");
                string destinationPath = Path.Combine(uploadsDir, uniqueFileName);

                try
                {
                    File.Copy(openFileDialog.FileName, destinationPath);
                    _uploadedFilePath = destinationPath;

                    // Display the uploaded file name (original file name)
                    UploadedFileNameTextBlock.Text = fileInfo.Name;

                    MessageBox.Show("Document uploaded successfully!", "Upload Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while uploading the document: {ex.Message}", "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CalculateAmount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (decimal.TryParse(HoursWorkedTextBox.Text, out decimal hoursWorked) &&
                    decimal.TryParse(HourlyRateTextBox.Text, out decimal hourlyRate))
                {
                    decimal totalAmount = hoursWorked * hourlyRate;
                    MessageBox.Show($"Calculated Amount: {totalAmount:C}", "Amount Calculation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Please enter valid numbers for Hours Worked and Hourly Rate.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during calculation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubmitClaimButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate fields
            if (string.IsNullOrWhiteSpace(LecturerIDTextBox.Text) ||
                string.IsNullOrWhiteSpace(LecturerNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(HoursWorkedTextBox.Text) ||
                string.IsNullOrWhiteSpace(HourlyRateTextBox.Text) ||
                !WorkDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in all fields before submitting the claim.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Parse and validate numeric fields
            if (!int.TryParse(LecturerIDTextBox.Text, out int lecturerID))
            {
                MessageBox.Show("Lecturer ID must be a valid integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(HoursWorkedTextBox.Text, out decimal hoursWorked))
            {
                MessageBox.Show("Hours Worked must be a valid number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(HourlyRateTextBox.Text, out decimal hourlyRate))
            {
                MessageBox.Show("Hourly Rate must be a valid number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Calculate Amount
            decimal totalAmount = hoursWorked * hourlyRate;

            // Create a new claim
            Claim newClaim = new Claim
            {
                LecturerID = lecturerID,
                LecturerName = LecturerNameTextBox.Text.Trim(),
                HoursWorked = (int)hoursWorked,
                HourlyRate = hourlyRate,
                Amount = (double)totalAmount,
                DateWorked = WorkDatePicker.SelectedDate.Value,
                Status = "Submitted",
                UploadedFileName = string.IsNullOrEmpty(_uploadedFilePath) ? "No file uploaded." : Path.GetFileName(_uploadedFilePath),
                UploadedFilePath = _uploadedFilePath,
                DateSubmitted = DateTime.Now
            };

            // Add the claim to the collection
            Claims.Add(newClaim);

            // Update UI
            ClaimStatusTextBlock.Text = "Submitted";
            ClaimStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);

            MessageBox.Show("Your claim has been successfully submitted!", "Submission Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear input fields
            ClearLecturerFields();
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem != null)
            {
                Claim selectedClaim = (Claim)ClaimsListView.SelectedItem;

                if (selectedClaim.Status != "Submitted" && selectedClaim.Status != "Rejected")
                {
                    MessageBox.Show("Only 'Submitted' or 'Rejected' claims can be approved.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selectedClaim.Status = "Approved";
                selectedClaim.DateReviewed = DateTime.Now;
                selectedClaim.ReviewerComments = string.Empty; // Clear previous comments if any

                ClaimsListView.Items.Refresh();
                MessageBox.Show("Claim approved successfully.", "Approval Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a claim to approve.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClaimsListView.SelectedItem != null)
            {
                Claim selectedClaim = (Claim)ClaimsListView.SelectedItem;

                if (selectedClaim.Status != "Submitted" && selectedClaim.Status != "Approved")
                {
                    MessageBox.Show("Only 'Submitted' or 'Approved' claims can be rejected.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Capture comments
                string comments = CommentsTextBox.Text.Trim();

                selectedClaim.Status = "Rejected";
                selectedClaim.DateReviewed = DateTime.Now;
                selectedClaim.ReviewerComments = string.IsNullOrEmpty(comments) ? "No comments provided." : comments;

                ClaimsListView.Items.Refresh();
                MessageBox.Show("Claim rejected successfully.", "Rejection Success", MessageBoxButton.OK, MessageBoxImage.Information);


                // Optionally, store comments or handle them as needed
                if (!string.IsNullOrEmpty(comments))
                {
                    // For demonstration, I've stored comments in the claim's ReviewerComments property
                    // You can extend this to log comments or notify the lecturer
                }

                // Clear comments after rejection
                CommentsTextBox.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Please select a claim to reject.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearLecturerFields()
        {
            LecturerIDTextBox.Text = string.Empty;
            LecturerNameTextBox.Text = string.Empty;
            HoursWorkedTextBox.Text = string.Empty;
            HourlyRateTextBox.Text = string.Empty;
            WorkDatePicker.SelectedDate = null;
            ClaimStatusTextBlock.Text = "Pending...";
            ClaimStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            UploadedFileNameTextBlock.Text = "No file uploaded.";
            _uploadedFilePath = string.Empty;
        }

        // Implement INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Handler to open the uploaded file when clicked
        private void UploadedFileNameTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ClaimsListView.SelectedItem is Claim selectedClaim && !string.IsNullOrEmpty(selectedClaim.UploadedFilePath))
            {
                try
                {
                    // Open the file using the default associated application
                    Process.Start(new ProcessStartInfo(selectedClaim.UploadedFilePath)
                    {
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to open the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Class to represent a claim
    public class Claim : INotifyPropertyChanged
    {
        private string _status;
        private string _reviewerComments;
        private DateTime? _dateReviewed;

        public int LecturerID { get; set; }
        public string LecturerName { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public double Amount { get; set; }
        public DateTime DateWorked { get; set; }

        public string UploadedFileName { get; set; }
        public string UploadedFilePath { get; set; }

        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        public DateTime DateSubmitted { get; set; }

        public DateTime? DateReviewed
        {
            get { return _dateReviewed; }
            set
            {
                if (_dateReviewed != value)
                {
                    _dateReviewed = value;
                    OnPropertyChanged("DateReviewed");
                }
            }
        }

        public string ReviewerComments
        {
            get { return _reviewerComments; }
            set
            {
                if (_reviewerComments != value)
                {
                    _reviewerComments = value;
                    OnPropertyChanged("ReviewerComments");
                }
            }
        }

        // Implement INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
