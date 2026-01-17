import sys
import os
import shutil

def get_base_path():
    """ Get absolute path to resource, works for dev and for PyInstaller """
    if getattr(sys, 'frozen', False):
        return os.path.dirname(sys.executable)
    else:
        return os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

def get_internal_path(relative_path):
    """ Get path to resource bundled inside _MEIPASS or source """
    if getattr(sys, 'frozen', False):
        # PyInstaller temp folder
        if hasattr(sys, '_MEIPASS'):
            return os.path.join(sys._MEIPASS, relative_path)
        return os.path.join(os.path.dirname(sys.executable), relative_path)
    else:
        # Dev mode
        return os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), relative_path)

def get_asset_path(relative_path):
    """ Get path for static assets (images, icons) - Read Only from Bundle """
    return get_internal_path(relative_path)

def get_config_path(filename):
    """ 
    Get path for config file. 
    1. Checks if file exists in 'config' folder next to EXE.
    2. If not, attempts to copy it from the internal bundle to the external folder.
    3. If copy fails (permissions), falls back to the internal bundled file.
    """
    base_folder = get_base_path()
    config_dir = os.path.join(base_folder, "config")
    target_file = os.path.join(config_dir, filename)
    
    # 1. If external exists, return it
    if os.path.exists(target_file):
        return target_file
    
    # 2. Try to copy from internal
    internal_file = get_internal_path(os.path.join("config", filename))
    
    if os.path.exists(internal_file):
        try:
            if not os.path.exists(config_dir):
                os.makedirs(config_dir)
            shutil.copy2(internal_file, target_file)
            return target_file # Return new external copy
        except OSError:
            pass # Failed to write (permissions?), fallback to internal
            
        return internal_file # Return internal read-only
        
    return target_file # Fallback to target (will likely fail to read if doesn't exist)
