   M S B A M L   `   `   `  ����  medica_center    medica_center.LoginWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Login��5   	   $	��300��6   $	��400��6$    � WindowStartupLocation$ CenterScreen=�60   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5	      $
��Auto �6   �� 5
      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��0��6!   $��	Username:��61   $��10q�6B   �� 5   
   -   5         ��Name$ UsernameTextBox��5   /   $��0��6   $��1��6   $��10 10 80 10q�6H   �� 5   
   $��1��6   $��0��6!   $��	Password:��61   $��10q�6B   G� 5   
   -   5         G�Name$ PasswordBox��5   3   $��1��6   $��1��6#   $��10 10 80 10q�6H   �� 5   
   $��2��6   $��0��6!   $��Role:��61   $��10q�6>   �� 5   
   -   5         ��Name$ RoleComboBox��5   0   $��2��6   $��1��6    $��10 10 80 10q�6F   .@�5      ��  �� 
IsSelected . .��6-   Administrator6<   �� 5      .��6   Client6$   �� 5      .��6   Doctor6$   5      �� 5   
   -   5        $��3��5      $��0��6   $��2��6.   $��Login��6B   $��100 20 100 10q�6l   �� 5   
   -   5        $��4��5      $��0��6   $��2��6.   $0��*Ещё не зарегистрирован��6B   $��100 0 100 10q�6�   5       5!      