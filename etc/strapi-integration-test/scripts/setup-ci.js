/**
 * Strapi Admin and API Token Setup Script for CI/CD
 * 自動建立管理員帳號和 API Token
 */

const axios = require('axios');
const fs = require('fs').promises;
const path = require('path');

const STRAPI_URL = 'http://localhost:1337';
const ADMIN_USER = {
  firstname: 'CI',
  lastname: 'Admin',
  email: 'ci-admin@test.com',
  password: 'CIAdmin123!'
};

const API_TOKEN_CONFIG = {
  name: `ci-integration-test-token-${Date.now()}`,
  description: 'API token for CI integration tests',
  type: 'full-access',
  permissions: null, // full-access 不需要明確權限
  lifespan: null
};

async function waitForStrapi(maxRetries = 30, interval = 2000) {
  console.log('🔍 Waiting for Strapi to be ready...');
  
  for (let i = 0; i < maxRetries; i++) {
    try {
      // 使用根路徑檢查，而不是 /admin
      const response = await axios.get(`${STRAPI_URL}/`, {
        timeout: 5000
      });
      // Strapi 根路徑通常會重定向到 /admin，所以 3xx 也是成功
      if (response.status >= 200 && response.status < 400) {
        console.log('✅ Strapi is ready!');
        return true;
      }
    } catch (error) {
      // 檢查是否是重定向到 /admin 的回應
      if (error.response && error.response.status >= 300 && error.response.status < 400) {
        console.log('✅ Strapi is ready! (received redirect)');
        return true;
      }
      console.log(`⏳ Attempt ${i + 1}/${maxRetries} - Strapi not ready yet...`);
      await new Promise(resolve => setTimeout(resolve, interval));
    }
  }
  
  throw new Error('❌ Strapi failed to start within timeout period');
}

async function createAdminUser() {
  console.log('👤 Creating admin user...');
  
  try {
    const response = await axios.post(
      `${STRAPI_URL}/admin/register-admin`,
      ADMIN_USER,
      {
        headers: {
          'Content-Type': 'application/json'
        },
        timeout: 10000
      }
    );
    
    const token = response.data?.data?.token || response.data?.token;
    
    if (!token) {
      throw new Error('No JWT token received from admin registration');
    }
    
    console.log('✅ Admin user created successfully');
    console.log(`🔑 JWT Token: ${token.substring(0, 20)}...`);
    
    return token;
  } catch (error) {
    if (error.response?.status === 400) {
      console.log('ℹ️ Admin user may already exist, trying to get existing token...');
      // 如果管理員已存在，嘗試登入
      return await loginAdminUser();
    }
    
    console.error('❌ Failed to create admin user:', error.response?.data || error.message);
    throw error;
  }
}

async function loginAdminUser() {
  console.log('🔑 Logging in as existing admin user...');
  
  try {
    const response = await axios.post(
      `${STRAPI_URL}/admin/login`,
      {
        email: ADMIN_USER.email,
        password: ADMIN_USER.password
      },
      {
        headers: {
          'Content-Type': 'application/json'
        },
        timeout: 10000
      }
    );
    
    const token = response.data?.data?.token || response.data?.token;
    
    if (!token) {
      throw new Error('No JWT token received from admin login');
    }
    
    console.log('✅ Admin login successful');
    return token;
  } catch (error) {
    console.error('❌ Failed to login admin user:', error.response?.data || error.message);
    throw error;
  }
}

async function setPublicPermissions(jwtToken) {
  console.log('🔓 設定完整 API 權限（包含 CRUD 和 Upload）...');
  
  try {
    // 設定完整的 CRUD 權限
    const permissions = {
      'api::article.article': ['find', 'findOne', 'create', 'update', 'delete'],
      'api::category.category': ['find', 'findOne', 'create', 'update', 'delete'],
      'api::author.author': ['find', 'findOne', 'create', 'update', 'delete'],
      'api::global.global': ['find', 'findOne', 'create', 'update', 'delete'],
      'api::about.about': ['find', 'findOne', 'create', 'update', 'delete'],
      'plugin::upload.upload': ['find', 'findOne', 'upload', 'destroy']
    };
    
    // 先取得所有可用的權限
    const permissionsResponse = await axios.get(`${STRAPI_URL}/admin/content-api/permissions`, {
      headers: {
        'Authorization': `Bearer ${jwtToken}`
      },
      timeout: 10000
    });
    
    console.log('📋 可用權限:', Object.keys(permissionsResponse.data?.data || {}));
    
    // 取得 public role
    const rolesResponse = await axios.get(`${STRAPI_URL}/admin/users-permissions/roles`, {
      headers: {
        'Authorization': `Bearer ${jwtToken}`
      },
      timeout: 10000
    });
    
    const publicRole = rolesResponse.data?.find(role => role.type === 'public');
    if (!publicRole) {
      console.log('⚠️ 無法找到 public role');
      return;
    }
    
    console.log('✅ 找到 public role:', publicRole.id);
    
    // 設定權限
    for (const [controller, actions] of Object.entries(permissions)) {
      for (const action of actions) {
        const permissionKey = `${controller}.${action}`;
        console.log(`🔑 設定權限: ${permissionKey}`);
        
        try {
          // 嘗試更新權限設定
          await axios.put(`${STRAPI_URL}/admin/users-permissions/roles/${publicRole.id}`, {
            permissions: {
              [controller]: {
                [action]: {
                  enabled: true
                }
              }
            }
          }, {
            headers: {
              'Authorization': `Bearer ${jwtToken}`,
              'Content-Type': 'application/json'
            },
            timeout: 5000
          });
          
          console.log(`✅ 權限設定成功: ${permissionKey}`);
        } catch (permError) {
          console.log(`⚠️ 權限 ${permissionKey} 設定失敗:`, permError.response?.data || permError.message);
        }
      }
    }
    
    console.log('✅ 權限設定完成');
  } catch (error) {
    console.log('⚠️ 權限設定失敗，但繼續進行:', error.response?.data || error.message);
  }
}

async function createApiToken(jwtToken) {
  console.log('🎫 Creating API token...');
  
  try {
    console.log('🎯 Creating full-access API token...');
    
    const response = await axios.post(
      `${STRAPI_URL}/admin/api-tokens`,
      API_TOKEN_CONFIG,
      {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${jwtToken}`
        },
        timeout: 10000
      }
    );
    
    const apiToken = response.data?.data?.accessKey || response.data?.accessKey;
    
    if (!apiToken) {
      throw new Error('No API token received from token creation');
    }
    
    console.log('✅ API token created successfully');
    console.log(`🔑 API Token: ${apiToken.substring(0, 20)}...`);
    console.log(`📋 Token type: ${response.data?.data?.type || 'unknown'}`);
    
    return apiToken;
  } catch (error) {
    console.error('❌ Failed to create API token:', error.response?.data || error.message);
    throw error;
  }
}

async function createTestConfig(apiToken) {
  console.log('📝 Creating test configuration...');
  
  const testConfig = {
    Strapi: {
      StrapiUrl: STRAPI_URL,
      StrapiToken: apiToken
    }
  };
  
  // 使用預設的 appsettings.json
  const configPath = path.join(process.cwd(), '../../Further.Strapi.Tests/appsettings.json');
  const configDir = path.dirname(configPath);
  
  try {
    // 確保目錄存在
    await fs.mkdir(configDir, { recursive: true });
    
    // 寫入配置檔案
    await fs.writeFile(configPath, JSON.stringify(testConfig, null, 2));
    
    console.log('✅ Test configuration created successfully');
    console.log(`📁 Config saved to: ${configPath}`);
    
    return configPath;
  } catch (error) {
    console.error('❌ Failed to create test configuration:', error.message);
    throw error;
  }
}

async function verifyApiAccess(apiToken) {
  console.log('🔍 Verifying API access...');
  
  try {
    const response = await axios.get(`${STRAPI_URL}/api/articles`, {
      headers: {
        'Authorization': `Bearer ${apiToken}`
      },
      timeout: 5000
    });
    
    console.log('✅ API access verified successfully');
    console.log(`📊 API Response: ${response.status} - ${JSON.stringify(response.data).substring(0, 100)}...`);
    
    return true;
  } catch (error) {
    console.error('❌ API access verification failed:', error.response?.data || error.message);
    throw error;
  }
}

async function main() {
  try {
    console.log('🚀 Starting Strapi CI setup...');
    
    // 1. 等待 Strapi 啟動
    await waitForStrapi();
    
    // 2. 建立管理員帳號
    const jwtToken = await createAdminUser();
    
    // 3. 設定基本 API 權限
    await setPublicPermissions(jwtToken);
    
    // 4. 建立 API Token
    const apiToken = await createApiToken(jwtToken);
    
    // 5. 建立測試配置
    await createTestConfig(apiToken);
    
    // 6. 驗證 API 存取
    await verifyApiAccess(apiToken);
    
    console.log('🎉 Strapi CI setup completed successfully!');
    console.log('📋 Summary:');
    console.log(`   - Admin User: ${ADMIN_USER.email}`);
    console.log(`   - API Token: ${apiToken.substring(0, 20)}...`);
    console.log(`   - Strapi URL: ${STRAPI_URL}`);
    
    process.exit(0);
  } catch (error) {
    console.error('💥 Setup failed:', error.message);
    process.exit(1);
  }
}

// 只在直接執行時運行
if (require.main === module) {
  main();
}

module.exports = {
  waitForStrapi,
  createAdminUser,
  createApiToken,
  createTestConfig,
  verifyApiAccess
};