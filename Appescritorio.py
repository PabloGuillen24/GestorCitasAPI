import tkinter as tk
from tkinter import ttk, messagebox
import requests
import json
from datetime import datetime

class GestorCitasApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Gestor de Citas - Cliente de Escritorio")
        self.root.geometry("1200x700")
        
        # Configuración de la API
        self.api_base_url = "http://localhost:5176/api"
        self.verify_ssl = False
        
        if not self.verify_ssl:
            import urllib3
            urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
        
        # Crear pestañas
        self.tab_control = ttk.Notebook(root)
        
        # Pestañas
        self.tabs = {
            'citas': ttk.Frame(self.tab_control),
            'clientes': ttk.Frame(self.tab_control),
            'profesionales': ttk.Frame(self.tab_control),
            'servicios': ttk.Frame(self.tab_control)
        }
        
        for name, tab in self.tabs.items():
            self.tab_control.add(tab, text=name.title())
        
        self.tab_control.pack(expand=1, fill="both")
        
        # Barra de estado
        self.status_var = tk.StringVar()
        self.status_bar = ttk.Label(root, textvariable=self.status_var, relief=tk.SUNKEN)
        self.status_bar.pack(side=tk.BOTTOM, fill=tk.X)
        
        # Inicializar las pestañas
        self.init_tabs()
        
        # Cargar datos iniciales
        self.status_var.set("Conectando a la API...")
        self.cargar_datos_iniciales()
    
    def init_tabs(self):
        # Configuración común para todas las pestañas
        for entity, tab in self.tabs.items():
            self.init_tab(entity, tab)
    
    def init_tab(self, entity, tab):
        # Frame para controles
        frame_controles = ttk.Frame(tab)
        frame_controles.pack(pady=10, padx=10, fill=tk.X)
        
        # Botones
        btn_nuevo = ttk.Button(frame_controles, text=f"Nuev{'a' if entity in ['citas', 'servicios'] else 'o'} {entity[:-1]}", 
                              command=lambda: self.mostrar_formulario(entity))
        btn_nuevo.pack(side=tk.LEFT, padx=5)
        
        btn_refrescar = ttk.Button(frame_controles, text="Refrescar", 
                                  command=lambda: self.cargar_entidad(entity))
        btn_refrescar.pack(side=tk.LEFT, padx=5)
        
        btn_eliminar = ttk.Button(frame_controles, text="Eliminar", 
                                 command=lambda: self.eliminar_entidad(entity))
        btn_eliminar.pack(side=tk.LEFT, padx=5)
        
        # Treeview
        frame_tabla = ttk.Frame(tab)
        frame_tabla.pack(pady=10, padx=10, fill=tk.BOTH, expand=True)
        
        # Columnas según la entidad
        columns = self.get_columns_for_entity(entity)
        treeview = ttk.Treeview(frame_tabla, columns=columns, show='headings')
        
        # Configurar columnas
        for col in columns:
            treeview.heading(col, text=col)
            treeview.column(col, width=100)
        
        # Scrollbar
        scrollbar = ttk.Scrollbar(frame_tabla, orient=tk.VERTICAL, command=treeview.yview)
        treeview.configure(yscroll=scrollbar.set)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        treeview.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        # Guardar referencia al treeview
        setattr(self, f'tree_{entity}', treeview)
        
        # Bind double click para editar
        treeview.bind('<Double-1>', lambda e, ent=entity: self.editar_entidad(ent))
    
    def get_columns_for_entity(self, entity):
        if entity == 'citas':
            return ('ID', 'Cliente', 'Profesional', 'Servicio', 'Fecha', 'Hora', 'Estado')
        elif entity == 'clientes':
            return ('ID', 'Nombre', 'Apellido', 'Email', 'Teléfono')
        elif entity == 'profesionales':
            return ('ID', 'Nombre', 'Apellido', 'Especialidad', 'Email', 'Teléfono')
        elif entity == 'servicios':
            return ('ID', 'Nombre', 'Descripción', 'Precio', 'Duración')
        return ()
    
    def hacer_peticion(self, endpoint, method='GET', data=None):
        url = f"{self.api_base_url}/{endpoint}"
        try:
            if method == 'GET':
                response = requests.get(url, verify=self.verify_ssl)
            elif method == 'POST':
                response = requests.post(url, json=data, verify=self.verify_ssl)
            elif method == 'PUT':
                response = requests.put(url, json=data, verify=self.verify_ssl)
            elif method == 'DELETE':
                response = requests.delete(url, verify=self.verify_ssl)
            
            return response
            
        except requests.exceptions.ConnectionError:
            messagebox.showerror("Error de Conexión", 
                               f"No se puede conectar a la API en:\n{url}\n\n"
                               "Asegúrate de que la API esté ejecutándose")
            return None
        except Exception as e:
            messagebox.showerror("Error", f"Error inesperado: {str(e)}")
            return None
    
    def cargar_datos_iniciales(self):
        for entity in self.tabs.keys():
            self.cargar_entidad(entity)
    
    def cargar_entidad(self, entity):
        self.status_var.set(f"Cargando {entity}...")
        response = self.hacer_peticion(entity)
        
        if response and response.status_code == 200:
            items = response.json()
            treeview = getattr(self, f'tree_{entity}')
            
            # Limpiar tabla
            for item in treeview.get_children():
                treeview.delete(item)
            
            # Llenar tabla
            for item in items:
                values = self.get_values_for_entity(entity, item)
                treeview.insert('', tk.END, values=values)
            
            self.status_var.set(f"{entity.title()} cargados: {len(items)} registros")
        elif response:
            messagebox.showerror("Error", f"Error al cargar {entity}: {response.status_code}")
    
    def get_values_for_entity(self, entity, item):
        if entity == 'citas':
            return (
                item.get('id', ''),
                item.get('clienteNombre', ''),
                item.get('profesionalNombre', ''),
                item.get('servicioNombre', ''),
                item.get('fecha', ''),
                item.get('hora', ''),
                item.get('estado', '')
            )
        elif entity == 'clientes':
            return (
                item.get('id', ''),
                item.get('nombre', ''),
                item.get('apellido', ''),
                item.get('email', ''),
                item.get('telefono', '')
            )
        elif entity == 'profesionales':
            return (
                item.get('id', ''),
                item.get('nombre', ''),
                item.get('apellido', ''),
                item.get('especialidad', ''),
                item.get('email', ''),
                item.get('telefono', '')
            )
        elif entity == 'servicios':
            return (
                item.get('id', ''),
                item.get('nombre', ''),
                item.get('descripcion', ''),
                item.get('precio', ''),
                item.get('duracion', '')
            )
        return ()
    
    def mostrar_formulario(self, entity, item=None):
        formulario = tk.Toplevel(self.root)
        formulario.title(f"{'Editar' if item else 'Nuevo'} {entity[:-1].title()}")
        formulario.geometry("400x400")
        
        # Frame principal
        frame_principal = ttk.Frame(formulario)
        frame_principal.pack(pady=20, padx=20, fill=tk.BOTH, expand=True)
        
        # Campos del formulario
        campos = self.get_campos_for_entity(entity)
        variables = {}
        
        for i, (campo, tipo, label) in enumerate(campos):
            ttk.Label(frame_principal, text=label).grid(row=i, column=0, sticky=tk.W, pady=5)
            
            if tipo == 'entry':
                var = tk.StringVar()
                entry = ttk.Entry(frame_principal, textvariable=var)
                entry.grid(row=i, column=1, sticky=tk.EW, pady=5, padx=5)
            elif tipo == 'combobox':
                var = tk.StringVar()
                combo = ttk.Combobox(frame_principal, textvariable=var, state="readonly")
                combo.grid(row=i, column=1, sticky=tk.EW, pady=5, padx=5)
            
            variables[campo] = var
        
        # Si estamos editando, llenar con datos existentes
        if item:
            self.llenar_formulario(entity, variables, item)
        
        # Cargar opciones para combobox si es necesario
        if entity == 'citas':
            self.cargar_opciones_combobox(variables)
        
        # Botones
        frame_botones = ttk.Frame(frame_principal)
        frame_botones.grid(row=len(campos), column=0, columnspan=2, pady=20)
        
        def guardar():
            datos = self.obtener_datos_formulario(entity, variables)
            if item:
                datos['id'] = item['id'] if isinstance(item, dict) else item[0]
                self.actualizar_entidad(entity, datos, formulario)
            else:
                self.crear_entidad(entity, datos, formulario)
        
        ttk.Button(frame_botones, text="Guardar", command=guardar).pack(side=tk.LEFT, padx=5)
        ttk.Button(frame_botones, text="Cancelar", command=formulario.destroy).pack(side=tk.LEFT, padx=5)
        
        # Configurar grid weights
        frame_principal.columnconfigure(1, weight=1)
        formulario.columnconfigure(0, weight=1)
        formulario.rowconfigure(0, weight=1)
    
    def get_campos_for_entity(self, entity):
        if entity == 'citas':
            return [
                ('clienteId', 'combobox', 'Cliente:'),
                ('profesionalId', 'combobox', 'Profesional:'),
                ('servicioId', 'combobox', 'Servicio:'),
                ('fecha', 'entry', 'Fecha:'),
                ('hora', 'entry', 'Hora:'),
                ('estado', 'combobox', 'Estado:')
            ]
        elif entity == 'clientes':
            return [
                ('nombre', 'entry', 'Nombre:'),
                ('apellido', 'entry', 'Apellido:'),
                ('email', 'entry', 'Email:'),
                ('telefono', 'entry', 'Teléfono:')
            ]
        elif entity == 'profesionales':
            return [
                ('nombre', 'entry', 'Nombre:'),
                ('apellido', 'entry', 'Apellido:'),
                ('especialidad', 'entry', 'Especialidad:'),
                ('email', 'entry', 'Email:'),
                ('telefono', 'entry', 'Teléfono:')
            ]
        elif entity == 'servicios':
            return [
                ('nombre', 'entry', 'Nombre:'),
                ('descripcion', 'entry', 'Descripción:'),
                ('precio', 'entry', 'Precio:'),
                ('duracion', 'entry', 'Duración (min):')
            ]
        return []
    
    def llenar_formulario(self, entity, variables, item):
        if entity == 'citas' and isinstance(item, dict):
            variables['clienteId'].set(item.get('clienteId', ''))
            variables['profesionalId'].set(item.get('profesionalId', ''))
            variables['servicioId'].set(item.get('servicioId', ''))
            variables['fecha'].set(item.get('fecha', ''))
            variables['hora'].set(item.get('hora', ''))
            variables['estado'].set(item.get('estado', ''))
        elif isinstance(item, (tuple, list)):
            # Asumir que es una tupla de valores del treeview
            pass
    
    def cargar_opciones_combobox(self, variables):
        # Cargar clientes
        response = self.hacer_peticion('clientes')
        if response and response.status_code == 200:
            clientes = response.json()
            opciones = [f"{c['id']}: {c['nombre']} {c['apellido']}" for c in clientes]
            variables['clienteId'].widget['values'] = opciones
        
        # Cargar profesionales
        response = self.hacer_peticion('profesionales')
        if response and response.status_code == 200:
            profesionales = response.json()
            opciones = [f"{p['id']}: {p['nombre']} {p['apellido']}" for p in profesionales]
            variables['profesionalId'].widget['values'] = opciones
        
        # Cargar servicios
        response = self.hacer_peticion('servicios')
        if response and response.status_code == 200:
            servicios = response.json()
            opciones = [f"{s['id']}: {s['nombre']}" for s in servicios]
            variables['servicioId'].widget['values'] = opciones
        
        # Estados para citas
        variables['estado'].widget['values'] = ['Programada', 'Confirmada', 'Completada', 'Cancelada']
    
    def obtener_datos_formulario(self, entity, variables):
        datos = {}
        for campo, var in variables.items():
            datos[campo] = var.get()
        return datos
    
    def crear_entidad(self, entity, datos, formulario):
        response = self.hacer_peticion(entity, 'POST', datos)
        if response and response.status_code in [200, 201]:
            messagebox.showinfo("Éxito", f"{entity[:-1].title()} creado correctamente")
            formulario.destroy()
            self.cargar_entidad(entity)
        elif response:
            messagebox.showerror("Error", f"Error al crear {entity}: {response.status_code}")
    
    def actualizar_entidad(self, entity, datos, formulario):
        response = self.hacer_peticion(f"{entity}/{datos['id']}", 'PUT', datos)
        if response and response.status_code == 200:
            messagebox.showinfo("Éxito", f"{entity[:-1].title()} actualizado correctamente")
            formulario.destroy()
            self.cargar_entidad(entity)
        elif response:
            messagebox.showerror("Error", f"Error al actualizar {entity}: {response.status_code}")
    
    def editar_entidad(self, entity):
        treeview = getattr(self, f'tree_{entity}')
        selection = treeview.selection()
        if not selection:
            messagebox.showwarning("Advertencia", f"Selecciona un {entity[:-1]} para editar")
            return
        
        item = selection[0]
        valores = treeview.item(item, 'values')
        
        # Para edición simple, mostramos el formulario con los valores actuales
        self.mostrar_formulario(entity, valores)
    
    def eliminar_entidad(self, entity):
        treeview = getattr(self, f'tree_{entity}')
        selection = treeview.selection()
        if not selection:
            messagebox.showwarning("Advertencia", f"Selecciona un {entity[:-1]} para eliminar")
            return
        
        item = selection[0]
        valores = treeview.item(item, 'values')
        item_id = valores[0]
        
        if messagebox.askyesno("Confirmar", f"¿Eliminar el {entity[:-1]} {item_id}?"):
            response = self.hacer_peticion(f"{entity}/{item_id}", 'DELETE')
            if response and response.status_code == 200:
                messagebox.showinfo("Éxito", f"{entity[:-1].title()} eliminado correctamente")
                self.cargar_entidad(entity)
            elif response:
                messagebox.showerror("Error", f"Error al eliminar {entity}: {response.status_code}")

if __name__ == "__main__":
    root = tk.Tk()
    app = GestorCitasApp(root)
    root.mainloop()