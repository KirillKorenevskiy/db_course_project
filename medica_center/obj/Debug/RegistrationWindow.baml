   M S B A M L   `   `   `  ����H  Dmedica_center, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:medica_centermedica_center  5   	   &     medica_center.RegistrationWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:medica_center     5   	     � Title$  RegistrationWindow��5   	   $	��300��6$   $	��400��61    � WindowStartupLocation$ CenterScreen=�6=   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��0��6!   $��First Name:��61   $��10q�6D   �� 5   
   -   5         ��Name$ FirstNameTextBox��5   /   $��0��6   $��1��6   $��10 10 80 10q�6I   �� 5   
   $��1��6   $��0��6!   $��
Last Name:��61   $��10q�6C   �� 5   
   -   5        $ LastNameTextBox��5   /   $��1��6   $��1��6   $��10 10 80 10q�6H   �� 5   
   $��2��6   $��0��6!   $��	Username:��61   $��10q�6B   �� 5   
   -   5        $ UsernameTextBox��5   /   $��2��6   $��1��6   $��10 10 80 10q�6H   �� 5    
   $��3��6   $��0��6!   $��	Password:��61   $��10q�6B   G� 5!   
   -   5         G�Name$ PasswordBox��5!   3   $��3��6   $��1��6#   $��10 10 80 10q�6H   �� 5#   
   -   5        $��5��5#      $��0��6   $��2��6.   $��Register��6B   $��100 20 100 10q�6r   5$      5%      