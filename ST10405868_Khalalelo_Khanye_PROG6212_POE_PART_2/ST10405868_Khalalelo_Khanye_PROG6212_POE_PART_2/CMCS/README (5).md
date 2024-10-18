 Contract Monthly Claim System (CMCS)

 Overview

The Contract Monthly Claim System (CMCS) is a WPF-based application that allows lecturers to submit claims for hours worked, which are then verified and approved by a coordinator or manager. The system includes features for document uploads, claim submission, approval, rejection, and claim status tracking.
 Features
- Lecturer Claim Submission: Lecturers can submit claims for the hours worked, along with supporting documents.
- Document Upload: Upload PDF, DOCX, or XLSX files as supporting documents. File size is limited to 5 MB.
- Claim Approval/Rejection: Coordinators or managers can approve or reject claims.
- Amount Calculation: Automatically calculate the total claim amount based on hours worked and the hourly rate.
- Claim Status Tracking: Claims can have statuses such as 'Submitted', 'Approved', or 'Rejected'.
- File Viewing: Uploaded documents can be viewed directly from the UI.

Prerequisites
- .NET Core SDK
- Visual Studio with WPF support
- Microsoft Windows OS
 How to Run
1. Open the solution in Visual Studio.
2. Restore NuGet packages if required.
3. Run the application by clicking the "Start" button in Visual Studio.

File Structure
- MainWindow.xaml.cs: The main logic for handling claim submissions, approvals, and document uploads.
- Claim.cs: Defines the `Claim` class that holds information about a claim.
- MainWindow.xaml: Defines the user interface (UI) layout and elements for the Lecturer and Coordinator/Manager tabs.

 How to Use
 Lecturer Tab:
1. Fill in Claim Details:
   - Enter `Lecturer ID`, `Lecturer Name`, `Hours Worked`, and `Hourly Rate`.
   - Select the date worked using the date picker.
   
2. Upload Supporting Documents:
   - Click the "Upload Document" button and choose a file (PDF, DOCX, or XLSX format, max size 5 MB).
   - The uploaded file name will be displayed after a successful upload.

3. Calculate Amount:
   - Click the "Calculate Amount" button to calculate the total amount based on the entered hours and hourly rate.

4. Submit Claim:
   - Click the "Submit Claim" button to submit the claim.
   - Ensure all fields are filled before submission.

Coordinator/Manager Tab:
1. View Claims:
   The list of submitted claims is shown in a list view, including the lecturer ID, name, hours worked, and uploaded file.
   
2. Approve/Reject Claims:
   Select a claim and click "Approve" or "Reject". Optionally, provide comments when rejecting a claim.
   
3. View Uploaded Documents:
  Click on the uploaded document file name to open the document using the default application associated with the file type.

 Error Handling
File Size Exceeded: If the uploaded document exceeds 5 MB, an error message will be displayed.
Invalid File Type: Only PDF, DOCX, and XLSX file types are allowed for uploads. Other formats will trigger an error.
Invalid Input: Lecturers are prompted with error messages for invalid inputs in fields like `Lecturer ID`, `Hours Worked`, and `Hourly Rate`.

Dependencies
Microsoft.Win32: Used for file selection dialogs.
System.Diagnostics: For opening files with the default application.
ObservableCollection: Used for data-binding the list of claims to the UI.
  
Future Enhancements
Role-Based Access: Separate roles and authentication for lecturers and coordinators/managers.
Claim History: Allow lecturers to view previously submitted claims and their statuses.
Notifications: Notify lecturers when claims are approved or rejected.

Contact
For any issues or feature requests, feel free to contact the development team.
