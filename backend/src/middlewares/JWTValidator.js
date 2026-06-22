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
                    Cookie: `RefreshToken=${refreshToken}`,
                },
                timeout: 5000,
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
        console.error(
            "[JWTValidator] Error al refrescar token:",
            error.response?.status || error.message
        );
        return null;
    }
}

/**
 * Establece las cookies de autenticación en la respuesta.
 */
function setAuthCookies(res, accessToken, refreshToken) {
    res.cookie("AuthToken", accessToken, {
        httpOnly: true,
        secure: true,
        sameSite: "Strict",
        path: "/",
        maxAge: 30 * 60 * 1000,
    });

    if (refreshToken) {
        res.cookie("RefreshToken", refreshToken, {
            httpOnly: true,
            secure: true,
            sameSite: "Strict",
            path: "/",
            maxAge: 7 * 24 * 60 * 60 * 1000,
        });
    }
}

/**
 * ✅ Función para extraer token de diferentes fuentes
 * 1. Cookie HttpOnly (AuthToken)
 * 2. Header Authorization: Bearer <token>
 * 3. Query param (para debugging)
 */
function extractToken(req) {
    // 1. Intentar desde cookie HttpOnly
    let token = req.cookies?.AuthToken;
    
    // 2. Si no hay cookie, intentar del header Authorization
    if (!token) {
        const authHeader = req.headers.authorization;
        if (authHeader && authHeader.startsWith('Bearer ')) {
            token = authHeader.substring(7);
            console.log("📥 Token extraído del header Authorization");
        }
    }
    
    // 3. Intentar desde query param (solo para debugging)
    if (!token && req.query?.token) {
        token = req.query.token;
        console.log("📥 Token extraído del query param");
    }
    
    return token;
}

/**
 * Middleware para validar JWT desde cookies HttpOnly o header Authorization.
 */
export const validateToken = async (req, res, next) => {
    // ✅ Extraer token de cualquier fuente
    const token = extractToken(req);

    console.log("🔑 TOKEN RECIBIDO:", token ? token.substring(0, 30) + "..." : "NINGUNO");
    console.log("🍪 Cookies:", req.cookies);
    console.log("📋 Header Authorization:", req.headers.authorization);
    console.log("🔐 JWT_SECRET:", JWT_SECRET ? JWT_SECRET.substring(0, 5) + "..." : "UNDEFINED");

    if (!token) {
        return res.status(401).json({ 
            message: "No autorizado - Token no proporcionado",
            source: "no-token"
        });
    }

    // Intentar verificar el token directamente
    try {
        const decoded = jwt.verify(token, JWT_SECRET, {
            algorithms: ["HS256"],
        });
        
        // ✅ Normalizar claims si vienen en formato URI
        const normalizedUser = {
            id: decoded.id || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
            email: decoded.email || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
            name: decoded.name || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
            role: decoded.role || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"],
            ...decoded
        };
        
        req.user = normalizedUser;
        console.log("✅ USUARIO AUTENTICADO:", req.user);
        return next();
    } catch (err) {
        console.error("[JWTValidator] Error:", err.name, err.message);

        if (err.name !== "TokenExpiredError") {
            return res.status(401).json({ 
                message: "Token inválido",
                error: err.message 
            });
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

        // Normalizar claims
        const normalizedUser = {
            id: decoded.id || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
            email: decoded.email || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
            name: decoded.name || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
            role: decoded.role || decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"],
            ...decoded
        };

        // Establecer las nuevas cookies en la respuesta
        setAuthCookies(res, newTokens.accessToken, newTokens.refreshToken);

        // Continuar con la request usando el nuevo payload
        req.user = normalizedUser;
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
};