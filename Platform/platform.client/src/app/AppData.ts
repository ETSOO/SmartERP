/**
 * Application data
 * 程序数据
 */
export type AppData = {
  /**
   * Id
   * 编号
   */
  id: number;

  /**
   * Application name
   * 程序名称
   */
  name: string;

  /**
   * Web URL
   * Web网址
   */
  webUrl: string;

  /**
   * Help URL
   * 帮助网址
   */
  helpUrl?: string;

  /**
   * Logo
   * 图标
   */
  logo?: string;
};
