//Middleware para realizar validacion de Json Web Token enviado por el login_service (.NET) atraves de HttpOnly cookie
//Si el token está expirado, intenta refrescarlo automáticamente llamando al login_service

import jwt from "jsonwebtoken";
import axios from "axios";
import dotenv from "dotenv";

dotenv.config();

const JWT_SECRET = process.env.JWT_SECRET;
const LOGIN_SERVICE_URL = process.env.LOGIN_SERVICE_URL || "http://localhost:5176";

/**
 * Intenta refrescar el token de acceso llamando al endpoint de refresh del login_service.
 * Retorna los nuevos tokens si es exitoso, o null si falla.
 */
async function refreshAccessToken(refreshToken) {
    try {
        const response = await axios.post(
            `${LOGIN_SERVICE_URL}/api/auth/refresh`,
            { refreshToken },
            {
                headers: {
                    "Content-Type": "application/json",
                    // Enviar el refresh token también como cookie para compatibilidad con el login_service
                    Cookie: `RefreshToken=${refreshToken}`,
                },
                timeout: 5000, // 5 segundos máximo de espera
            }
        );

        if (response.status === 200 && response.data?.accessToken) {
            return {
                accessToken: response.data.accessToken,
                refreshToken: response.data.refreshToken,
            };
        }

        return null;
    } catch (error) {
        // Log discreto sin exponer información sensible
        console.error(
            "[JWTValidator] Error al refrescar token:",
            error.response?.status || error.message
        );
        return null;
    }
}

/**
 * Establece las cookies de autenticación en la respuesta, alineadas con la configuración
 * del CookieHelper del login_service (.NET).
 */
function setAuthCookies(res, accessToken, refreshToken) {
    // Cookie del access token — alineada con CookieHelper.SetSecureJwtCookie
    res.cookie("AuthToken", accessToken, {
        httpOnly: true,
        secure: true,
        sameSite: "Strict",
        path: "/",
        maxAge: 30 * 60 * 1000, // 30 minutos
    });

    // Cookie del refresh token — alineada con CookieHelper.SetSecureRefreshCookie
    if (refreshToken) {
        res.cookie("RefreshToken", refreshToken, {
            httpOnly: true,
            secure: true,
            sameSite: "Strict",
            path: "/",
            maxAge: 7 * 24 * 60 * 60 * 1000, // 7 días
        });
    }
}

/**
 * Middleware para validar JWT desde cookies HttpOnly.
 *
 * Flujo:
 * 1. Lee el token de la cookie "AuthToken"
 * 2. Verifica firma y expiración
 * 3. Si el token expiró y existe "RefreshToken", intenta refrescar automáticamente
 * 4. Si el refresh es exitoso, actualiza las cookies y continúa la request
 */
export const validateToken = async (req, res, next) => {
    const token = req.cookies?.AuthToken;
    
    
    console.log("TOKEN RECIBIDO:", token ? token.substring(0, 20) + "..." : "NINGUNO");
    console.log("JWT_SECRET:", JWT_SECRET ? JWT_SECRET.substring(0, 5) + "..." : "UNDEFINED");


    if (!token) {
        return res.status(401).json({ message: "No autorizado" });
    }

    // Intentar verificar el token directamente
    try {
        const decoded = jwt.verify(token, JWT_SECRET, {
            algorithms: ["HS256"],
            issuer: process.env.ISSUER,
            audience: process.env.AUDIENCE,
        });
        req.user = decoded;
        console.log("USUARIO DECODIFICADO: ", req.user)
        return next();
    } catch (err) {
        // Si el error NO es de expiración, el token es inválido (firma corrupta, malformado, etc.)
        console.error("[JWTValidator] Error:", err.name, err.message);

        if (err.name !== "TokenExpiredError") {
            return res.status(403).json({ message: "Token inválido" });
        }
    }

    // --- El token está expirado, intentar refresh automático ---
    const refreshToken = req.cookies?.RefreshToken;

    if (!refreshToken) {
        return res.status(401).json({
            message: "Token expirado",
            expired: true,
        });
    }

    // Llamar al login_service para obtener nuevos tokens
    const newTokens = await refreshAccessToken(refreshToken);

    if (!newTokens) {
        return res.status(401).json({
            message: "Sesión expirada, inicie sesión nuevamente",
            expired: true,
        });
    }

    // Verificar que el nuevo token sea válido antes de usarlo
    try {
        const decoded = jwt.verify(newTokens.accessToken, JWT_SECRET, {
            algorithms: ["HS256"],
        });

        // Establecer las nuevas cookies en la respuesta
        setAuthCookies(res, newTokens.accessToken, newTokens.refreshToken);

        // Continuar con la request usando el nuevo payload
        req.user = normalizeClaims(decoded);
        return next();
    } catch (verifyErr) {
        console.error(
            "[JWTValidator] El nuevo token del refresh no es válido:",
            verifyErr.message
        );
        return res
            .status(401)
            .json({ message: "Error al renovar sesión", expired: true });
    }

    //Helper para los claims
    function normalizeClaims(decoded){
        const resultado = {...decoded};
        for (const [uri, alias] of Object.entries(CLAIMS_MAP)) {
            if (decoded[uri]) {
                resultado[alias] = decoded[uri];
                delete resultado[uri];
            }
        }
        return resultado;
    }

    const CLAIM_MAP = {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "id",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "email",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "name",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role": "role",
};
};
