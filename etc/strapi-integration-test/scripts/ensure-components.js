#!/usr/bin/env node

const fs = require('fs').promises;
const path = require('path');

// 共用組件定義
const SHARED_COMPONENTS = {
  'shared/string-item': {
    collectionName: 'components_shared_string_items',
    info: {
      displayName: 'StringItem'
    },
    options: {},
    attributes: {
      value: {
        type: 'string'
      }
    },
    config: {}
  },
  'shared/media': {
    collectionName: 'components_shared_media',
    info: {
      displayName: 'Media',
      icon: 'file-video'
    },
    attributes: {
      file: {
        type: 'media',
        multiple: false,
        required: false,
        allowedTypes: ['images', 'files', 'videos', 'audios']
      }
    }
  },
  'shared/slider': {
    collectionName: 'components_shared_sliders',
    info: {
      displayName: 'Slider',
      icon: 'layer-group'
    },
    attributes: {
      files: {
        type: 'media',
        multiple: true,
        required: false,
        allowedTypes: ['images']
      }
    }
  }
};

async function ensureSharedComponents() {
  console.log('📦 Ensuring shared components exist...');
  
  for (const [componentPath, componentSchema] of Object.entries(SHARED_COMPONENTS)) {
    const [category, name] = componentPath.split('/');
    const componentDir = path.join(process.cwd(), 'src', 'components', category);
    const componentFile = path.join(componentDir, `${name}.json`);
    
    try {
      // 檢查組件檔案是否存在
      await fs.access(componentFile);
      console.log(`✅ Component already exists: ${componentPath}`);
    } catch {
      // 檔案不存在，建立組件
      console.log(`📝 Creating component: ${componentPath}`);
      
      try {
        // 確保目錄存在
        await fs.mkdir(componentDir, { recursive: true });
        
        // 寫入組件檔案
        await fs.writeFile(componentFile, JSON.stringify(componentSchema, null, 2));
        
        console.log(`✅ Component created successfully: ${componentPath}`);
      } catch (createError) {
        console.error(`❌ Failed to create component ${componentPath}:`, createError.message);
        throw createError;
      }
    }
  }
  
  console.log('✅ All shared components ensured');
}

async function main() {
  try {
    console.log('🔧 Pre-Strapi component setup...');
    await ensureSharedComponents();
    console.log('🎉 Pre-Strapi component setup completed!');
  } catch (error) {
    console.error('❌ Pre-Strapi component setup failed:', error.message);
    process.exit(1);
  }
}

main();