import os
import uuid

# 1. Чтение всех версий из файла all_version.txt
def get_versions(file_path):
    with open(file_path, 'r') as file:
        versions = file.readlines()
    return [version.strip() for version in versions]

# 2. Просим пользователя выбрать версию
def choose_version(versions):
    print("Доступные версии:")
    for idx, version in enumerate(versions, 1):
        print(f"{idx}. {version}")
    choice = int(input("Введите номер версии: ")) - 1
    return versions[choice]

# 3. Спрашиваем, нужно ли выбрать тип версии или модлоадер
def choose_version_type():
    print("Выберите тип версии:")
    print("1. Последний релиз")
    print("2. Последний снапшот")
    print("3. Конкретная версия (например, 1.20.1)")
    version_type = input("Введите номер типа версии: ")
    if version_type == '1':
        return "release"
    elif version_type == '2':
        return "snapshot"
    elif version_type == '3':
        version = input("Введите версию (например, 1.20.1): ")
        return version
    else:
        print("Неверный выбор.")
        return choose_version_type()

# 4. Спрашиваем, нужен ли модлоадер
def choose_mod_loader(version):
    print("Выберите модлоадер (оставьте пустым, если не нужно):")
    print("1. Fabric")
    print("2. Quilt")
    print("3. Forge")
    print("4. NeoForge")
    mod_loader = input("Введите 1 для Fabric, 2 для Quilt, 3 для Forge, 4 для NeoForge или нажмите Enter для пропуска: ")
    if mod_loader == '1':
        return f"fabric:{version}" if version else "fabric"
    elif mod_loader == '2':
        return f"quilt:{version}" if version else "quilt"
    elif mod_loader == '3':
        return f"forge:{version}" if version else "forge"
    elif mod_loader == '4':
        return f"neoforge:{version}" if version else "neoforge"
    else:
        return version  # Если модлоадер не выбран

# 5. Запуск Minecraft с выбранной версией и именем пользователя
def run_minecraft(version_type, username, user_uuid):
    # Получаем путь к текущей директории
    current_dir = os.path.dirname(os.path.abspath(__file__))
    # Строим полный путь к папке .minecraft
    minecraft_dir = os.path.join(current_dir, 'minecraft')
    
    # Выносим аргументы JVM в отдельную переменную
    jvm_args = (
        "-Xmx2G "
        "-XX:+UnlockExperimentalVMOptions "
        "-XX:+UseG1GC "
        "-XX:ParallelGCThreads=4 "
        "-XX:+UseStringDeduplication "
        "-XX:+OptimizeStringConcat "
        "-XX:ConcGCThreads=4 "
        "-XX:MaxInlineLevel=15 "
        "-Xmn1G "
        "-XX:+UseCompressedOops"
    )
    
    # Формируем команду. Обратите внимание, что для передачи jvm_args заключены двойные кавычки.
    command = (
        f'.\\pypoetry\\venv\\Scripts\\poetry.exe run portablemc '
        f'--main-dir "{minecraft_dir}" '
        f'--work-dir "{minecraft_dir}" '
        f'start "--jvm-args={jvm_args}" {version_type} -u {username} -i {user_uuid}'
    )
    
    print(f"Запуск команды: {command}")
    os.system(command)

# Основной процесс
def main():
    # Запрашиваем имя пользователя
    username = input("Введите ваше имя пользователя для Minecraft: ")
    
    # Генерация UUID, если пользователь не указал
    user_uuid = str(uuid.uuid5(uuid.NAMESPACE_DNS, username))
    
    file_path = 'all_version.txt'  # Путь к файлу с версиями
    
    # Получаем список версий
    versions = get_versions(file_path)
    
    if not versions:
        print("Файл версий пуст или не найден.")
        return
    
    # Выбираем версию или тип версии
    version_type = choose_version_type()
    
    # Если выбрана конкретная версия, проверяем, что она есть в списке
    if version_type not in ("release", "snapshot") and version_type not in versions:
        print("Версия не найдена.")
        return
    
    # Выбираем модлоадер (если требуется)
    version_type = choose_mod_loader(version_type)
    
    # Запускаем Minecraft с передачей имени пользователя и UUID
    run_minecraft(version_type, username, user_uuid)

if __name__ == "__main__":
    main()
