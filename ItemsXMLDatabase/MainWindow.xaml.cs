using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;

namespace ItemsXMLDatabase
{
    public partial class MainWindow : Window
    {
        //Вспомогательное поле для извлечения выбранного элемента
        ObservableCollection<ItemsClass> parentOfSelected = null; 
        String currentFilePath = null; //Путь к текущему файлу
        XmlSerializer xmlSerializer = //Сериализатор
            new XmlSerializer(typeof(ObservableCollection<ItemsClass>));
        ObservableCollection<ItemsClass> itemsBase = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveToFile()
        {
            using (FileStream fs = new FileStream(currentFilePath, FileMode.Create))
                xmlSerializer.Serialize(fs, itemsBase);
        }

        private void NewItemCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Новый предмет
            ItemsClass selected = twItemsClasses.SelectedItem as ItemsClass;
            AddItem addItemWnd = new AddItem(); //Диалоговое окно добавления предмета
            Item result = addItemWnd.ShowDialog();
            if (result != null) selected.Items.Add(result);
            SaveToFile();
        }

        private void RemoveSelectedItemCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Удалить предмет
            (twItemsClasses.SelectedItem as ItemsClass).
                Items.Remove(dgItems.SelectedItem as Item);
            SaveToFile();
        }

        private void NewItemCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Разрешение добавить новый предмет
            e.CanExecute = false;
            if (currentFilePath != null)
                if (twItemsClasses.SelectedItem != null)
                    e.CanExecute = true;
        }

        private void RemoveSelectedItemCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Разрешение удалить предмет
            e.CanExecute = false;
            if (currentFilePath != null)
                if (dgItems.SelectedItem != null)
                    e.CanExecute = true;
        }

        private void NewItemsClassCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Новый класс предметов
            ItemsClass selected = twItemsClasses.SelectedItem as ItemsClass;
            AddItemsClass addItemsClassWnd = new AddItemsClass();
            String result = addItemsClassWnd.ShowDialog();
            if ( result != String.Empty)
                selected.ItemsClasses.Add(new ItemsClass(result));
            SaveToFile();
        }
        

        private void FindParentOfSelected( //Рекурсивный обход дерева
             ObservableCollection<ItemsClass> itemsBase, ItemsClass selected)
        {
            if (itemsBase.Contains(selected)) parentOfSelected = itemsBase;
            else 
                foreach (ItemsClass iClass in itemsBase)
                    FindParentOfSelected(iClass.ItemsClasses, selected);
        }
       
        private void RemoveSelectedItemsClassCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Удалить выделенный класс предметов
            ItemsClass selected = twItemsClasses.SelectedItem as ItemsClass;
            FindParentOfSelected(itemsBase, selected);
            parentOfSelected.Remove(selected);
            twItemsClasses.ItemsSource = itemsBase;
            SaveToFile();
        }
        
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Выход
            Application.Current.Shutdown(0);
        }
        
        private void NewFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Создать новый файл
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            if (saveFileDlg.ShowDialog() ?? false)
            {
                currentFilePath = saveFileDlg.FileName;
                itemsBase = new ObservableCollection<ItemsClass>();
                itemsBase.Add(new ItemsClass("Категории"));
                twItemsClasses.ItemsSource = itemsBase;
            }
            SaveToFile();
        }

        private void OpenFileCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Открыть файл
            OpenFileDialog openFileDlg = new OpenFileDialog();
            if (openFileDlg.ShowDialog() ?? false)
            {
                currentFilePath = openFileDlg.FileName;
                try
                {
                    using (FileStream fs = new FileStream(currentFilePath, FileMode.Open))
                        itemsBase = xmlSerializer.Deserialize(fs) as ObservableCollection<ItemsClass>;
                    twItemsClasses.ItemsSource = itemsBase;
                }
                catch
                {
                    MessageBox.Show("Ошибка открытия файла");
                }
            }
        }

        private void NewItemsClassCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Резрешено ли добавить класс предметов
            e.CanExecute = false;
            if (currentFilePath != null)
                if (twItemsClasses.SelectedItem != null)
                    e.CanExecute = true;
        }

        private void RemoveSelectedItemsClassCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Разрешено ли удалить класс предметов
            e.CanExecute = false;
            if(itemsBase != null)
                if(twItemsClasses.SelectedItem != null)
                    if((twItemsClasses.SelectedItem as ItemsClass) != itemsBase.First()) //Корневый нельзя
                        e.CanExecute = true;
        }

        private void twItemsClasses_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //Выбран другой класс
            ItemsClass selected = twItemsClasses.SelectedItem as ItemsClass;
            dgItems.ItemsSource = selected.Items;
        }
    }

    public static class CustomCommand //Команды для приложения
    {
        public static readonly RoutedUICommand
            NewItemsClass =
                new RoutedUICommand("Добавить новый класс бьектов в выделенную категорию",
                    "NewItemsClass", typeof(MainWindow)),
            RemoveSelectedItemsClass =
                new RoutedUICommand("Удалить выделенный класс обьектов",
                    "RemoveSelectedItemsClass", typeof(MainWindow)),
                    NewItem =
                new RoutedUICommand("Добавить новый обьектов",
                    "NewItem", typeof(MainWindow)),
                    RemoveSelectedItem =
                new RoutedUICommand("Удалить обьектов",
                    "RemoveSelectedItem", typeof(MainWindow));
    }

    [Serializable]
    public class Item
    {
        String id = "0";
        String name = "Без имени";

        public Item() { }
        public Item(String id, String name)
        {
            this.id = id;
            this.name = name;
        }

        [XmlAttribute] //Метаданные
        public String ID
        {
            set { id = value; }
            get { return id; }
        }

        [XmlAttribute]
        public String Name
        {
            set { name = value; }
            get { return name; }
        }
    }
    [Serializable]
    public class ItemsClass
    {
        String name;
        ObservableCollection<ItemsClass> itemsClasses = new ObservableCollection<ItemsClass>();
        ObservableCollection<Item> items = new ObservableCollection<Item>();

        [XmlArray]
        public ObservableCollection<Item> Items
        {
            set { items = value; }
            get { return items; }
        }
        [XmlAttribute]
        public String Name
        {
            set { name = value; }
            get { return name; }
        }

        [XmlArray]
        public ObservableCollection<ItemsClass> ItemsClasses
        {
            set { itemsClasses = value; }
            get { return itemsClasses; }
        }

        public ItemsClass() { }

        public ItemsClass(String name = "Без имени")
        {
            this.name = name;
        }
    }
}
