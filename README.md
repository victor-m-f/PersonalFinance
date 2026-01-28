# PersonalFinance

PersonalFinance is a lightweight Windows desktop application designed to help you track personal expenses in a simple, clear, and structured way.

Built with **WPF** and packaged using **MSIX**, the app runs locally on your computer and stores data on your own machine, giving you full control over your financial information.

## ✨ Features

- Create and manage expense categories and subcategories  
- Register daily expenses with date, description, and amount  
- Visual dashboard with spending summaries and trends  
- **Automatic expense parsing using OCR + LLM (LLaMA)**  
  - Upload receipts (PDF, PNG, JPG)
  - Extract date, amount, description, and suggested category
  - Review and confirm before saving  
- Local-first: no cloud, no accounts, no tracking  
- Clean and minimal UI focused on usability  

## 🧠 Intelligent Parsing (OCR + LLM)

PersonalFinance includes an optional intelligent parsing flow that uses:

- **OCR** to extract text from receipts and invoices  
- **LLaMA-based LLM** to interpret the content and suggest:
  - Expense amount
  - Date
  - Description
  - Best matching category based on existing data  

All processing happens locally or under user control.  
Nothing is sent automatically to third-party services.

## 🖥️ Technology Stack

- .NET (WPF)
- SQLite
- OCR (document text extraction)
- LLaMA (LLM-based semantic parsing)
- MSIX packaging
- WPF UI components

## 🔐 Privacy

PersonalFinance does **not** collect, transmit, or sell personal data.

- No accounts
- No analytics
- No background tracking  
- All data remains on the user's device

## 🏪 Microsoft Store

You can download PersonalFinance from the Microsoft Store:

👉 **Store link:**  
`<ADD MICROSOFT STORE LINK HERE>`

## 🚀 Installation

The application is distributed via the **Microsoft Store** using MSIX.  
Once installed, it runs as a standard Windows desktop application.

## 📌 Status

This is the **first public release** of the project.  
The focus is stability, clarity, and local-first usage.

## 📄 License

This project is licensed under the MIT License.
