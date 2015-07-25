#pragma once
#include "SCPExtensions.h"

namespace SCPUser {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;

	static SCP_EXTN m_Extension[4];
	static DWORD    m_Status   [4];

	public ref class SCPExtended : public System::Windows::Forms::Form
	{

	public:

		SCPExtended(void)
		{
			InitializeComponent();

			for(int Index = 0; Index < 4; Index++)
			{
				m_Status[Index] = ERROR_DEVICE_NOT_CONNECTED;
			}

			tmUpdate->Enabled = true;
		}

	protected:

		~SCPExtended()
		{
			if (components)
			{
				delete components;
			}
		}

	#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required designer variable.
		/// </summary>
	private: System::ComponentModel::IContainer^  components;
	private: System::Windows::Forms::ListView^  lvGrid;
	private: System::Windows::Forms::ColumnHeader^  chName;
	private: System::Windows::Forms::ColumnHeader^  chController1;
	private: System::Windows::Forms::ColumnHeader^  chController2;
	private: System::Windows::Forms::ColumnHeader^  chController3;
	private: System::Windows::Forms::Timer^  tmUpdate;
	private: System::Windows::Forms::ColumnHeader^  chController4;
	private:
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::Windows::Forms::ListViewItem^  listViewItem1 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Up", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem2 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Right", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem3 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Down", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem4 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Left", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem5 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"LX", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem6 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"LY", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem7 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"L1", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem8 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"L2", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem9 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"L3", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem10 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"RX", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem11 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"RY", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem12 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"R1", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem13 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"R2", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem14 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"R3", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem15 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Triangle", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem16 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Circle", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem17 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Cross", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem18 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Square", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem19 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Select", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem20 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"Start", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			System::Windows::Forms::ListViewItem^  listViewItem21 = (gcnew System::Windows::Forms::ListViewItem(gcnew cli::array< System::String^  >(5) {L"PS", 
				L"<n/a>", L"<n/a>", L"<n/a>", L"<n/a>"}, -1));
			this->lvGrid = (gcnew System::Windows::Forms::ListView());
			this->chName = (gcnew System::Windows::Forms::ColumnHeader());
			this->chController1 = (gcnew System::Windows::Forms::ColumnHeader());
			this->chController2 = (gcnew System::Windows::Forms::ColumnHeader());
			this->chController3 = (gcnew System::Windows::Forms::ColumnHeader());
			this->chController4 = (gcnew System::Windows::Forms::ColumnHeader());
			this->tmUpdate = (gcnew System::Windows::Forms::Timer(this->components));
			this->SuspendLayout();
			// 
			// lvGrid
			// 
			this->lvGrid->Columns->AddRange(gcnew cli::array< System::Windows::Forms::ColumnHeader^  >(5) {this->chName, this->chController1, 
				this->chController2, this->chController3, this->chController4});
			this->lvGrid->Dock = System::Windows::Forms::DockStyle::Fill;
			this->lvGrid->FullRowSelect = true;
			this->lvGrid->Items->AddRange(gcnew cli::array< System::Windows::Forms::ListViewItem^  >(21) {listViewItem1, listViewItem2, 
				listViewItem3, listViewItem4, listViewItem5, listViewItem6, listViewItem7, listViewItem8, listViewItem9, listViewItem10, listViewItem11, 
				listViewItem12, listViewItem13, listViewItem14, listViewItem15, listViewItem16, listViewItem17, listViewItem18, listViewItem19, 
				listViewItem20, listViewItem21});
			this->lvGrid->Location = System::Drawing::Point(0, 0);
			this->lvGrid->Name = L"lvGrid";
			this->lvGrid->Size = System::Drawing::Size(379, 388);
			this->lvGrid->TabIndex = 0;
			this->lvGrid->TabStop = false;
			this->lvGrid->UseCompatibleStateImageBehavior = false;
			this->lvGrid->View = System::Windows::Forms::View::Details;
			// 
			// chName
			// 
			this->chName->Text = L"Name";
			this->chName->Width = 75;
			// 
			// chController1
			// 
			this->chController1->Text = L"Value (#1)";
			this->chController1->Width = 75;
			// 
			// chController2
			// 
			this->chController2->Text = L"Value (#2)";
			this->chController2->Width = 75;
			// 
			// chController3
			// 
			this->chController3->Text = L"Value (#3)";
			this->chController3->Width = 75;
			// 
			// chController4
			// 
			this->chController4->Text = L"Value (#4)";
			this->chController4->Width = 75;
			// 
			// tmUpdate
			// 
			this->tmUpdate->Tick += gcnew System::EventHandler(this, &SCPExtended::tmUpdate_Tick);
			// 
			// SCPExtended
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(379, 388);
			this->ControlBox = false;
			this->Controls->Add(this->lvGrid);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedToolWindow;
			this->Name = L"SCPExtended";
			this->ShowInTaskbar = false;
			this->SizeGripStyle = System::Windows::Forms::SizeGripStyle::Hide;
			this->Text = L"SCPExtended";
			this->ResumeLayout(false);

		}
	#pragma endregion

	private: 
		System::Void tmUpdate_Tick(System::Object^ sender, System::EventArgs^ e)
		{
			SCP_EXTN Extension;
			DWORD Status;

			// lvGrid->BeginUpdate();

			for (int Index = 0, Column = 1, Row = 0; Index < 4; Index++, Column++)
			{
				if ((Status = XInputGetExtended(Index, &Extension)) == ERROR_SUCCESS)
				{
					if (m_Status[Index] != Status)
					{
						lvGrid->Items[ 0]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_UP   );
						lvGrid->Items[ 1]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RIGHT);
						lvGrid->Items[ 2]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_DOWN );
						lvGrid->Items[ 3]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LEFT );

						lvGrid->Items[ 4]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LX);
						lvGrid->Items[ 5]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LY);

						lvGrid->Items[ 6]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L1);
						lvGrid->Items[ 7]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L2);
						lvGrid->Items[ 8]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L3);

						lvGrid->Items[ 9]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RX);
						lvGrid->Items[10]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RY);

						lvGrid->Items[11]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R1);
						lvGrid->Items[12]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R2);
						lvGrid->Items[13]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R3);

						lvGrid->Items[14]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_T);
						lvGrid->Items[15]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_C);
						lvGrid->Items[16]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_X);
						lvGrid->Items[17]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_S);

						lvGrid->Items[18]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_SELECT);
						lvGrid->Items[19]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_START );

						lvGrid->Items[20]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_PS );
					}
					else
					{
						if (m_Extension[Index].SCP_UP    != Extension.SCP_UP   ) lvGrid->Items[0]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_UP   );
						if (m_Extension[Index].SCP_RIGHT != Extension.SCP_RIGHT) lvGrid->Items[1]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RIGHT);
						if (m_Extension[Index].SCP_DOWN  != Extension.SCP_DOWN ) lvGrid->Items[2]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_DOWN );
						if (m_Extension[Index].SCP_LEFT  != Extension.SCP_LEFT ) lvGrid->Items[3]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LEFT );

						if (m_Extension[Index].SCP_LX != Extension.SCP_LX) lvGrid->Items[4]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LX);
						if (m_Extension[Index].SCP_LY != Extension.SCP_LY) lvGrid->Items[5]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_LY);

						if (m_Extension[Index].SCP_L1 != Extension.SCP_L1) lvGrid->Items[6]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L1);
						if (m_Extension[Index].SCP_L2 != Extension.SCP_L2) lvGrid->Items[7]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L2);
						if (m_Extension[Index].SCP_L3 != Extension.SCP_L3) lvGrid->Items[8]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_L3);

						if (m_Extension[Index].SCP_RX != Extension.SCP_RX) lvGrid->Items[ 9]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RX);
						if (m_Extension[Index].SCP_RY != Extension.SCP_RY) lvGrid->Items[10]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_RY);

						if (m_Extension[Index].SCP_R1 != Extension.SCP_R1) lvGrid->Items[11]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R1);
						if (m_Extension[Index].SCP_R2 != Extension.SCP_R2) lvGrid->Items[12]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R2);
						if (m_Extension[Index].SCP_R3 != Extension.SCP_R3) lvGrid->Items[13]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_R3);

						if (m_Extension[Index].SCP_T != Extension.SCP_T) lvGrid->Items[14]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_T);
						if (m_Extension[Index].SCP_C != Extension.SCP_C) lvGrid->Items[15]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_C);
						if (m_Extension[Index].SCP_X != Extension.SCP_X) lvGrid->Items[16]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_X);
						if (m_Extension[Index].SCP_S != Extension.SCP_S) lvGrid->Items[17]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_S);

						if (m_Extension[Index].SCP_SELECT != Extension.SCP_SELECT) lvGrid->Items[18]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_SELECT);
						if (m_Extension[Index].SCP_START  != Extension.SCP_START ) lvGrid->Items[19]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_START );

						if (m_Extension[Index].SCP_PS  != Extension.SCP_PS ) lvGrid->Items[20]->SubItems[Column]->Text = String::Format("{0:F3}", Extension.SCP_PS );
					}

					memcpy(&m_Extension[Index], &Extension, sizeof(SCP_EXTN));
				}
				else
				{
					if (m_Status[Index] != Status)
					{
						for (int Row = 0; Row < lvGrid->Items->Count; Row++)
						{
							lvGrid->Items[Row]->SubItems[Column]->Text = String::Format("<n/a>");
						}
					}
				}

				m_Status[Index] = Status;
			}

			// lvGrid->EndUpdate();
		}
	};
}
