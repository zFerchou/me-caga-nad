﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Web.Services;
using System.Drawing;
using System.IO;

public partial class index : System.Web.UI.Page
{
    private ERPManagementDataContext erp;
    private storedProcedure sp;
    private view_ValidarUsuario validarUsuario;
    private ERPManagementRHDataContext erprh;
    //private static Object sessionCaptcha;
    protected void Page_Load(object sender, EventArgs e)
    {
        //sessionCaptcha= Session["Captcha"] = "";
        string EncodedResponse = Request.Form["g-Recaptcha-Response"];
        if (!IsPostBack)
        {
            UpdateCaptchaText();
        }
    }

    protected void btnAceptar_Click(object sender, EventArgs e)
    {
        //if (recaptcha.IsValid)
        //{

        erp = new ERPManagementDataContext();
        sp = new storedProcedure();
        validarUsuario = new view_ValidarUsuario();
        if (txtNomUsuario.Text != "" && txtContrasena.Text != "")
        {
            string pass = sp.getSha512(txtContrasena.Text);

            var usuario = (from vUser in erp.vUsuariosERPM
                           where vUser.usuario == txtNomUsuario.Text && vUser.contrasena == pass
                           select vUser).SingleOrDefault();

            if (!Object.ReferenceEquals(null, usuario))
            {
                var idUsu = (from usu in erp.vUsuariosERPM
                             where usu.usuario == txtNomUsuario.Text
                             select new { idUsuario = usu.idEmpleado }).First();

                var validacion = (from vUser in erp.view_ValidarUsuario
                                  where vUser.usuario == txtNomUsuario.Text && vUser.contrasena == pass && vUser.idUsuario == int.Parse(idUsu.idUsuario)
                                  select vUser).Count();

                if (validacion <= 0)
                {

                    lblError.Visible = true;
                    lblError.Text = "<div class='center width98 bg-alert' id='errorLogin'><span id='icon-25' class='warning blanco'></span>NO TIENES PERMISOS DE ACCESO</div>";
                }
                else
                {
                    Session["logged"] = true;
                    Session["username"] = usuario.nombreCompleto;
                    Session["id"] = usuario.idEmpleado;
                    insertaLog(int.Parse(usuario.idEmpleado));
                    Response.Redirect("Inicio.aspx");
                }
            }
            else
            {
                lblError.Visible = true;
                lblError.Text = "<div class='center width98 bg-alert' id='errorLogin'><span id='icon-25' class='warning blanco'></span>USUARIO/CONTRASEÑA INCORRECTOS</div>";
            }


        }
        else
        {
            lblError.Visible = true;
            lblError.Text = "<div class='center width98 bg-alert' id='errorLogin'><span id='icon-25' class='warning blanco'></span>DEBE AGREGAR USUARIO Y CONTRASEÑA</div>";
        }
        txtNomUsuario.Text = "";
        txtContrasena.Text = "";

        //else {
        //    lblError.Visible = true;
        //    lblError.Text = "<div class='center width98 bg-alert' id='errorLogin'><span id='icon-25' class='warning blanco'></span>Ingresa correctamente los datos de la imágen</div>";
        //}
    }

    public static void insertaLog(int idUsuario)
    {
        ERPManagementDataContext erp = new ERPManagementDataContext();
        tLogIngresoERPM tlog = new tLogIngresoERPM();

        DateTime fechaIngreso = DateTime.Now;
        tlog.idUsuario = idUsuario;
        tlog.fechaIngreso = fechaIngreso;
        erp.tLogIngresoERPM.InsertOnSubmit(tlog);
        erp.SubmitChanges();
    }

    protected void btnSubmitCaptcha_Click(object sender, EventArgs e)
    {
        //lblWait.Visible = true;
        //lblWait.Text = "Espera por favor...";
        //lblWait.ForeColor = System.Drawing.Color.Green;
        bool success = false;
        if (Session["Captcha"] != null)
        {
            //Match captcha text entered by user and the one stored in session
            if (Convert.ToString(Session["Captcha"]) == txtCaptchaText.Text.Trim())
            {
                success = true;
            }
        }


        if (success)
        {

            if (txtUsuario.Text != "")
            {
                erprh = new ERPManagementRHDataContext();
                var nomGrupo = (from tr in erprh.tUsuarios
                                where tr.usuario == txtUsuario.Text
                                select tr).FirstOrDefault();
                recuperarContrasenia(nomGrupo.usuario);
                txtUsuario.Text = "";
                txtCaptchaText.Text = "";
                btnSubmitCaptcha.Enabled = false;
                lblErrorCaptcha.Visible = true;
                lblErrorCaptcha.Text = "<div class='center width98 bg-alert' id='divSuccess' style='background-color:green;font-size: 11px;'><span id='icon-25' class='success blanco'></span>TE ENVIAMOS UNA NUEVA CONTRASEÑA A TU CORREO.</div>";
                lblErrorCaptcha.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblErrorCaptcha.Visible = true;
                lblErrorCaptcha.Text = "<div class='center width98 bg-alert' id='divSuccess' style='font-size: 11px;'><span id='icon-25' class='warning blanco'></span>Ingresa el usuario.</div>";
            }
        }
        else
        {
            UpdateCaptchaText();
            lblErrorCaptcha.Visible = true;
            lblErrorCaptcha.Text = "<div class='center width98 bg-alert' id='divError' style='font-size: 11px;'><span id='icon-25' class='warning blanco'></span>CÓDIGO INCORRECTO.</div>";
            lblErrorCaptcha.ForeColor = System.Drawing.Color.Red;
        }
    }

    protected void btnReGenerate_Click(object sender, EventArgs e)
    {
        UpdateCaptchaText();

    }

    private void UpdateCaptchaText()
    {
        txtCaptchaText.Text = string.Empty;
        lblError.Visible = false;
        //1) genera el codigo del captcha al cargar el index y lo guarda en Session["Captcha"].
        Session["Captcha"] = Guid.NewGuid().ToString().Substring(0, 6);
        if (IsPostBack)
        {
            lblError.Visible = true;
            Session["Captcha"] = Guid.NewGuid().ToString().Substring(0, 6);
            UpdatePanel1.Update();
            lblNewCaptcha.Visible = true;
            lblNewCaptcha.Text = "<p class='mensaje'>" + Convert.ToString(Session["Captcha"] = Guid.NewGuid().ToString().Substring(0, 6)) + "</p>";
            //string text = "<label style='font - size: 40px; color: white;font - family: Verdana;font - weight: 100;'>"+ Convert.ToString(Session["Captcha"] = Guid.NewGuid().ToString().Substring(0, 6)) + "</label>";
        }
    }

    public void recuperarContrasenia(string usuario)
    {
        ControllerIndex index = new ControllerIndex();
        index.recuperarContrasenia(usuario);
    }

    //Nuevas Funciones
    [WebMethod(EnableSession = true)]
    public static List<string> iniciaSessionString(string pass, string user)
    {
        /****Declaración de Variables****/
        #region variables
        storedProcedure sp = new storedProcedure();
        string query = "";
        List<string> session = new List<string>();
        string respuesta = "";
        ERPManagementRHDataContext sistemas = new ERPManagementRHDataContext();
        #endregion
        /*******************************/
        try
        {
            //Encriptamos la contraseña
            pass = sp.getSha512(pass);
            //Obtenemos coincidencias
            var usuario = (from vu in sistemas.vUsuarioAcceso
                           where vu.usuario == user
                           select vu).FirstOrDefault();
            view_ValidarUsuario validarUsuario = new view_ValidarUsuario();
            ERPManagementDataContext er = new ERPManagementDataContext();
            HttpContext.Current.Session["logged"] = true; // para DDBB Local

            /*
            var validacion = (from vUser in er.view_ValidarUsuario
                              where vUser.idUsuario == usuario.idUsuario
                              select vUser).Count();
            if (validacion > 0)
            {
                    //Obtenemos coincidencias
                    var getUser = (from vu in sistemas.vUsuarioAcceso
                                   where vu.usuario == user
                                   select vu).FirstOrDefault();
                    if (getUser.contrasena == pass)
                    {
                        //Validamos que el usuario exista con esa contraseña
                        var usuarios = (from vu in sistemas.vUsuarioAcceso
                                        where vu.usuario == user && vu.contrasena == pass
                                        select new { vu.idEstatusUsuario, vu.nombre, vu.apellidoPaterno, vu.idUsuario });
                        foreach (var usu in usuarios)
                        {
                            if (usu.idEstatusUsuario == 1)//Si cambió la contraseña (Nuevo)
                            {
                                HttpContext.Current.Session["logged"] = true;
                                HttpContext.Current.Session["username"] = usu.nombre + " " + usu.apellidoPaterno;
                                HttpContext.Current.Session["id"] = usu.idUsuario.ToString();
                                respuesta = "Nuevo";
                            }
                            else if (usu.idEstatusUsuario == 2)//Si esta Vigente
                            {
                                HttpContext.Current.Session["logged"] = true;
                                HttpContext.Current.Session["username"] = usu.nombre + " " + usu.apellidoPaterno;
                                HttpContext.Current.Session["id"] = usu.idUsuario.ToString();
                                insertaLog(int.Parse(usu.idUsuario.ToString()));
                                respuesta = "Activo";
                            }
                            else if (usu.idEstatusUsuario != 1 || usu.idEstatusUsuario != 4)
                            {
                                HttpContext.Current.Session["logged"] = true;
                                HttpContext.Current.Session["username"] = usu.nombre + " " + usu.apellidoPaterno;
                                HttpContext.Current.Session["id"] = usu.idUsuario.ToString();
                                respuesta = "Otro";
                            }


                            0***************Asignamos las credenciales a la lista****************0
                            session.Add(usu.nombre + " " + usu.apellidoPaterno);//nombre
                            session.Add(usu.idUsuario.ToString());//id
                            session.Add(usu.idEstatusUsuario.ToString());//status
                            session.Add(":)");//coinciden
                            session.Add(":)");//existe
                            session.Add(":)");//activo
                            0********************************************************************0
                        }//End for
                    }
                    else {
                        session.Add(":'(");//nombre
                        session.Add(":'(");//id
                        session.Add(":'(");//status
                        session.Add(":'(");//coinciden
                        session.Add(":'(");//existe
                        session.Add(":'(");//activo
                    }//End If                
            }//End If Validation
            */
        }
        catch (Exception ex)
        {
            /*Si marca excepción*/
            session.Add(":'(");//nombre
            session.Add(":'(");//id
            session.Add(":'(");//status
            session.Add(":'(");//coinciden
            session.Add(":'(");//existe
            session.Add(":'(");//activo
        }
        return session;
    }

    [WebMethod]
    public static string cambiarContrasenia(int idUsuario, string contrasenia)
    {
        ERPManagementRHDataContext rh = new ERPManagementRHDataContext();
        storedProcedure sp = new storedProcedure();
        try
        {
            //Encriptar la contraseña.
            string newPassword = sp.getSha512(contrasenia);
            var getUsuario = (from tr in rh.tUsuarios
                              where tr.idUsuario == idUsuario
                              select tr).FirstOrDefault();
            //Setear la nueva contraseña encriptada al campo "constrasena" para actualizarla.
            getUsuario.contrasena = newPassword;
            getUsuario.idEstatusUsuario = 2;
            //Actualizar la contraseña.
            rh.SubmitChanges();
            return "Cotraseña actualizada correctamente. °Correcto";
        }
        catch (Exception ex)
        {
            return "Error al cambiar tu contraseña. °Error";
        }
    }
}